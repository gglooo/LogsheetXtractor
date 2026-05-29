using FluentAssertions;
using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Moq;

namespace LogsheetXtractor.UnitTests.Application.Features.ExtractedValues;

public class GetNextLogsheetUnverifiedExtractedValuesTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoUnverifiedValuesExist()
    {
        var result = await GetNextLogsheetUnverifiedExtractedValuesHandler.Handle(
            new GetNextLogsheetUnverifiedExtractedValuesQuery(),
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        _mapperMock.Verify(m => m.Map<ExtractedValueDto>(It.IsAny<ExtractedValue>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFirstMatchingUnverifiedValue_ByLogsheetAndCreatedAt()
    {
        var templateA = CreateTemplate(Guid.NewGuid(), "Template-A");
        var templateB = CreateTemplate(Guid.NewGuid(), "Template-B");
        var deletedTemplate = CreateTemplate(Guid.NewGuid(), "Template-Deleted");
        deletedTemplate.DeletedAt = DateTime.UtcNow;

        var logsheetA = new Logsheet
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000010"),
            TemplateId = templateA.Id,
            Template = templateA,
            FileId = Guid.NewGuid(),
            File = CreateFile("logsheet-a.pdf"),
            Status = ELogSheetStatus.NeedsReview,
        };
        var logsheetB = new Logsheet
        {
            Id = Guid.Parse("00000000-0000-0000-0000-000000000020"),
            TemplateId = templateB.Id,
            Template = templateB,
            FileId = Guid.NewGuid(),
            File = CreateFile("logsheet-b.pdf"),
            Status = ELogSheetStatus.NeedsReview,
        };
        var deletedLogsheet = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = templateA.Id,
            Template = templateA,
            FileId = Guid.NewGuid(),
            File = CreateFile("logsheet-deleted.pdf"),
            Status = ELogSheetStatus.NeedsReview,
            DeletedAt = DateTime.UtcNow,
        };
        var logsheetWithDeletedTemplate = new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = deletedTemplate.Id,
            Template = deletedTemplate,
            FileId = Guid.NewGuid(),
            File = CreateFile("logsheet-with-deleted-template.pdf"),
            Status = ELogSheetStatus.NeedsReview,
        };

        var roiA = CreateRoi(templateA, "roi-a");
        var roiB = CreateRoi(templateB, "roi-b");
        var roiDeletedTemplate = CreateRoi(deletedTemplate, "roi-deleted-template");

        var ignoredVerified = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheetA.Id,
            Logsheet = logsheetA,
            RoiId = roiA.Id,
            Roi = roiA,
            Value = "verified",
            Status = EVerificationStatus.Verified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-5),
        };

        var expectedEntity = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheetA.Id,
            Logsheet = logsheetA,
            RoiId = roiA.Id,
            Roi = roiA,
            Value = "first-unverified",
            Status = EVerificationStatus.Unverified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-4),
        };

        var laterInSameLogsheet = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheetA.Id,
            Logsheet = logsheetA,
            RoiId = roiA.Id,
            Roi = roiA,
            Value = "later-unverified",
            Status = EVerificationStatus.Unverified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-1),
        };

        var unverifiedInLaterLogsheet = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheetB.Id,
            Logsheet = logsheetB,
            RoiId = roiB.Id,
            Roi = roiB,
            Value = "later-logsheet",
            Status = EVerificationStatus.Unverified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10),
        };

        var unverifiedInDeletedLogsheet = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = deletedLogsheet.Id,
            Logsheet = deletedLogsheet,
            RoiId = roiA.Id,
            Roi = roiA,
            Value = "deleted-logsheet",
            Status = EVerificationStatus.Unverified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-20),
        };

        var unverifiedInDeletedTemplate = new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheetWithDeletedTemplate.Id,
            Logsheet = logsheetWithDeletedTemplate,
            RoiId = roiDeletedTemplate.Id,
            Roi = roiDeletedTemplate,
            Value = "deleted-template",
            Status = EVerificationStatus.Unverified,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
        };

        _dbContext.Templates.AddRange(templateA, templateB, deletedTemplate);
        _dbContext.Rois.AddRange(roiA, roiB, roiDeletedTemplate);
        _dbContext.Logsheets.AddRange(
            logsheetA,
            logsheetB,
            deletedLogsheet,
            logsheetWithDeletedTemplate
        );
        _dbContext.ExtractedValues.AddRange(
            ignoredVerified,
            expectedEntity,
            laterInSameLogsheet,
            unverifiedInLaterLogsheet,
            unverifiedInDeletedLogsheet,
            unverifiedInDeletedTemplate
        );
        await _dbContext.SaveChangesAsync();

        var expectedDto = new ExtractedValueDto(
            expectedEntity.Id,
            expectedEntity.LogsheetId,
            expectedEntity.RoiId,
            expectedEntity.Roi.Type,
            expectedEntity.Roi.VariableName,
            expectedEntity.Value,
            null,
            EVerificationStatus.Unverified,
            expectedEntity.CreatedAt,
            expectedEntity.UpdatedAt
        );

        _mapperMock
            .Setup(m => m.Map<ExtractedValueDto>(It.Is<ExtractedValue>(e => e.Id == expectedEntity.Id)))
            .Returns(expectedDto);

        var result = await GetNextLogsheetUnverifiedExtractedValuesHandler.Handle(
            new GetNextLogsheetUnverifiedExtractedValuesQuery(),
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _mapperMock.Verify(
            m => m.Map<ExtractedValueDto>(It.Is<ExtractedValue>(e => e.Id == expectedEntity.Id)),
            Times.Once
        );
    }

    private static LogsheetXtractor.Domain.Entities.Template CreateTemplate(Guid fileId, string name)
    {
        return new LogsheetXtractor.Domain.Entities.Template
        {
            Name = name,
            FileId = fileId,
            File = CreateFile($"{name}.pdf"),
        };
    }

    private static Roi CreateRoi(LogsheetXtractor.Domain.Entities.Template template, string variableName)
    {
        return new Roi
        {
            TemplateId = template.Id,
            Template = template,
            VariableName = variableName,
            Type = ERoiType.Number,
            Coordinates = new Coordinates(1, 1, 10, 10),
        };
    }

    private static LogsheetXtractor.Domain.Entities.File CreateFile(string fileName)
    {
        return new LogsheetXtractor.Domain.Entities.File
        {
            OriginalFileName = fileName,
            StoredFileName = fileName,
            StoragePath = $"storage/{fileName}",
            ContentType = "application/pdf",
            SizeBytes = 100,
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
