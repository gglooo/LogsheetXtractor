using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using LogsheetXtractor.Application.Features.Template;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Application.Interfaces;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Template;

public class DetectRoisTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IRoiService> _roiServiceMock;

    public DetectRoisTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapperMock = new Mock<IMapper>();
        _roiServiceMock = new Mock<IRoiService>();
    }

    [Fact]
    public async Task Handle_ShouldReturnRois_WhenTemplateFound()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Test Template",
            FileId = file.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var detectedRois = new List<RoiDto>
        {
            new(
                Guid.NewGuid(),
                "ROI 1",
                template.Id,
                ERoiType.Handwritten,
                new Coordinates(10, 10, 100, 50),
                DateTime.UtcNow,
                null
            ),
            new(
                Guid.NewGuid(),
                "ROI 2",
                template.Id,
                ERoiType.Checkbox,
                new Coordinates(20, 20, 50, 50),
                DateTime.UtcNow,
                null
            ),
        };

        var responseDto = new DetectRoisResponseDto(detectedRois, []);

        _roiServiceMock
            .Setup(x =>
                x.DetectRoisAsync(
                    It.Is<LogsheetXtractor.Domain.Entities.Template>(t => t.Id == template.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(responseDto);

        var command = new DetectRoisCommand(template.Id);

        var result = await DetectRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(responseDto);
        _roiServiceMock.Verify(
            x =>
                x.DetectRoisAsync(
                    It.Is<LogsheetXtractor.Domain.Entities.Template>(t => t.Id == template.Id),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenTemplateNotFound()
    {
        var command = new DetectRoisCommand(Guid.NewGuid());

        var result = await DetectRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Template not found");
        _roiServiceMock.Verify(
            x =>
                x.DetectRoisAsync(
                    It.IsAny<LogsheetXtractor.Domain.Entities.Template>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnError_WhenServiceFails()
    {
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = "test.pdf",
            StoredFileName = "test.pdf",
            StoragePath = "path",
            ContentType = "application/pdf",
        };
        _dbContext.Files.Add(file);
        await _dbContext.SaveChangesAsync();

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Name = "Test Template",
            FileId = file.Id,
        };
        _dbContext.Templates.Add(template);
        await _dbContext.SaveChangesAsync();

        var errorMessage = "Service failure";
        _roiServiceMock
            .Setup(x =>
                x.DetectRoisAsync(
                    It.Is<LogsheetXtractor.Domain.Entities.Template>(t => t.Id == template.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception(errorMessage));

        var command = new DetectRoisCommand(template.Id);

        var result = await DetectRoisHandler.Handle(
            command,
            _roiServiceMock.Object,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be(errorMessage);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
