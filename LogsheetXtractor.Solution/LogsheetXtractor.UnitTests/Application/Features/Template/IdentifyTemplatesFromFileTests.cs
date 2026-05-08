using FluentAssertions;
using LogsheetXtractor.Application.Features.File.Interfaces;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class IdentifyTemplatesFromFileTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IFileService> _fileServiceMock = new();
    private readonly Mock<IPdfQrCodeScanner> _qrCodeScannerMock = new();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldFail_WhenFileDoesNotExist()
    {
        var result = await IdentifyTemplatesFromFileHandler.Handle(
            new IdentifyTemplatesFromFileQuery(Guid.NewGuid()),
            _dbContext,
            _fileServiceMock.Object,
            _qrCodeScannerMock.Object,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File not found");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenFileStreamIsMissing()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "source.pdf",
            StoredFileName = "source.pdf",
            StoragePath = "storage/source.pdf",
            ContentType = "application/pdf",
            SizeBytes = 77,
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        _fileServiceMock
            .Setup(fs => fs.GetFileAsync(file.Id))
            .ReturnsAsync((LogsheetXtractor.Application.DTOs.GetFileDto?)null);

        var result = await IdentifyTemplatesFromFileHandler.Handle(
            new IdentifyTemplatesFromFileQuery(file.Id),
            _dbContext,
            _fileServiceMock.Object,
            _qrCodeScannerMock.Object,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "File stream not found");
    }

    [Fact]
    public async Task Handle_ShouldMapOnlyTemplatesThatExistInDatabase()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "source.pdf",
            StoredFileName = "source.pdf",
            StoragePath = "storage/source.pdf",
            ContentType = "application/pdf",
            SizeBytes = 77,
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var templateFileA = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "A.pdf",
            StoredFileName = "A.pdf",
            StoragePath = "storage/A.pdf",
            ContentType = "application/pdf",
            SizeBytes = 10,
        };
        var templateFileB = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "B.pdf",
            StoredFileName = "B.pdf",
            StoragePath = "storage/B.pdf",
            ContentType = "application/pdf",
            SizeBytes = 10,
        };
        _dbContext.Files.AddRange(templateFileA, templateFileB);
        await _dbContext.SaveChangesAsync();

        var templateA = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Temperature",
            FileId = templateFileA.Id,
            File = templateFileA,
        };
        var templateB = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Pressure",
            FileId = templateFileB.Id,
            File = templateFileB,
        };
        _dbContext.Templates.AddRange(templateA, templateB);
        await _dbContext.SaveChangesAsync();

        _fileServiceMock
            .Setup(fs => fs.GetFileAsync(file.Id))
            .ReturnsAsync(
                new LogsheetXtractor.Application.DTOs.GetFileDto
                {
                    FileName = "source.pdf",
                    ContentType = "application/pdf",
                    Stream = new MemoryStream([1, 2, 3]),
                }
            );
        _qrCodeScannerMock
            .Setup(scanner => scanner.DetectTemplates(It.IsAny<byte[]>()))
            .Returns(
                new Dictionary<int, string>
                {
                    [0] = "Temperature",
                    [1] = "Unknown",
                    [2] = "",
                    [3] = "Pressure",
                    [4] = "Temperature",
                }
            );

        _mapperMock
            .Setup(m => m.Map<TemplateListDto>(It.Is<LogsheetXtractor.Domain.Entities.Template>(t => t.Id == templateA.Id)))
            .Returns(new TemplateListDto(templateA.Id, templateA.Name, null, null, templateA.FileId, 0, 0, 0, 0, DateTime.UtcNow));
        _mapperMock
            .Setup(m => m.Map<TemplateListDto>(It.Is<LogsheetXtractor.Domain.Entities.Template>(t => t.Id == templateB.Id)))
            .Returns(new TemplateListDto(templateB.Id, templateB.Name, null, null, templateB.FileId, 0, 0, 0, 0, DateTime.UtcNow));

        var result = await IdentifyTemplatesFromFileHandler.Handle(
            new IdentifyTemplatesFromFileQuery(file.Id),
            _dbContext,
            _fileServiceMock.Object,
            _qrCodeScannerMock.Object,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Keys.Should().BeEquivalentTo([0, 3, 4]);
        result.Value[0].Name.Should().Be("Temperature");
        result.Value[3].Name.Should().Be("Pressure");
        result.Value[4].Name.Should().Be("Temperature");
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
