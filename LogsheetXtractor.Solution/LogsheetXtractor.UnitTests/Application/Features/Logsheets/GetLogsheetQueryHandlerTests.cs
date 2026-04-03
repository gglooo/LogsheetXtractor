using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Common.Mappings;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.File.DTOs;
using LogsheetXtractor.Application.Features.Logsheets;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.Template.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.Logsheets;

public class GetLogsheetQueryHandlerTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnLogsheet_WhenLogsheetExists()
    {
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Template",
            File = new LogsheetXtractor.Domain.Entities.File { StoredFileName = "t.pdf" },
        };
        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            StoredFileName = "l.pdf",
        };

        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            Template = template,
            File = file,
            Status = LogsheetXtractor.Domain.Enums.ELogSheetStatus.Pending,
        };
        _dbContext.Logsheets.Add(logsheet);
        await _dbContext.SaveChangesAsync();

        var query = new GetLogsheetQuery(logsheet.Id);

        var expectedDto = new LogsheetDetailDto(
            logsheet.Id,
            new TemplateListDto(
                template.Id,
                template.Name,
                null,
                null,
                null,
                0,
                0,
                0,
                0,
                DateTime.UtcNow
            ),
            new FileDto(
                file.Id,
                file.OriginalFileName,
                file.ContentType,
                file.SizeBytes,
                file.CreatedAt
            ),
            logsheet.Status,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            null
        );

        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(It.IsAny<Logsheet>())).Returns(expectedDto);

        var result = await GetLogsheetHandler.Handle(
            query,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(expectedDto);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetNotFound()
    {
        var query = new GetLogsheetQuery(Guid.NewGuid());

        var result = await GetLogsheetHandler.Handle(
            query,
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<NotFoundError>();
        result.Errors.First().Message.Should().Be("Logsheet not found");
    }

    [Fact]
    public async Task Handle_ShouldReturnLogsheetWithRoiVariableNames_WhenLogsheetExists_WithRealMapper()
    {
        var config = new TypeAdapterConfig();
        new MappingConfig().Register(config);
        var mapper = new Mapper(config);

        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Template",
            File = new LogsheetXtractor.Domain.Entities.File
            {
                Id = Guid.NewGuid(),
                StoredFileName = "t.pdf",
            },
        };

        var roi = new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            VariableName = "TestVariable",
            Type = LogsheetXtractor.Domain.Enums.ERoiType.Handwritten,
            Coordinates = new LogsheetXtractor.Domain.ValueObjects.Coordinates(1, 1, 10, 10),
        };

        var file = new LogsheetXtractor.Domain.Entities.File
        {
            Id = Guid.NewGuid(),
            StoredFileName = "l.pdf",
        };

        var logsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            FileId = file.Id,
            File = file,
            Status = LogsheetXtractor.Domain.Enums.ELogSheetStatus.Pending,
        };

        var extractedValue = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheet.Id,
            RoiId = roi.Id,
            Value = "ExtractedText",
            Status = LogsheetXtractor.Domain.Enums.EVerificationStatus.Unverified,
        };

        _dbContext.Templates.Add(template);
        _dbContext.Rois.Add(roi);
        _dbContext.Files.Add(file);
        _dbContext.Logsheets.Add(logsheet);
        _dbContext.ExtractedValues.Add(extractedValue);
        await _dbContext.SaveChangesAsync();

        var query = new GetLogsheetQuery(logsheet.Id);

        _dbContext.ChangeTracker.Clear();

        var result = await GetLogsheetHandler.Handle(
            query,
            _dbContext,
            mapper,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.ExtractedValues.Should().NotBeEmpty();
        var dto = result.Value.ExtractedValues.First();

        dto.VariableName.Should().Be("TestVariable");
        dto.RoiId.Should().Be(roi.Id);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
