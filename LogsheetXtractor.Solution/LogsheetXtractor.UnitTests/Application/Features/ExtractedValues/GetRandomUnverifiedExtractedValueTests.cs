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

public class GetRandomUnverifiedExtractedValueTests : IDisposable
{
    private readonly AppDbContext _dbContext = TestDbContextFactory.Create();
    private readonly Mock<IMapper> _mapperMock = new();

    [Fact]
    public async Task Handle_ShouldReturnNull_WhenNoEligibleUnverifiedValuesExist()
    {
        var result = await GetRandomUnverifiedExtractedValueHandler.Handle(
            new GetRandomUnverifiedExtractedValueQuery(),
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
        _mapperMock.Verify(m => m.Map<ExtractedValueDto>(It.IsAny<ExtractedValue>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldExcludeVerifiedDeletedLogsheetAndDeletedTemplateValues()
    {
        var activeTemplate = CreateTemplate("active-template");
        var deletedTemplate = CreateTemplate("deleted-template");
        deletedTemplate.DeletedAt = DateTime.UtcNow;

        var activeLogsheet = CreateLogsheet(activeTemplate, "active-logsheet");
        var deletedLogsheet = CreateLogsheet(activeTemplate, "deleted-logsheet");
        deletedLogsheet.DeletedAt = DateTime.UtcNow;
        var logsheetWithDeletedTemplate = CreateLogsheet(deletedTemplate, "deleted-template-logsheet");

        var activeRoi = CreateRoi(activeTemplate, "active-roi");
        var deletedTemplateRoi = CreateRoi(deletedTemplate, "deleted-template-roi");

        var expected = CreateExtractedValue(activeLogsheet, activeRoi, EVerificationStatus.Unverified, "expected");
        var verified = CreateExtractedValue(activeLogsheet, activeRoi, EVerificationStatus.Verified, "verified");
        var deletedLogsheetValue = CreateExtractedValue(deletedLogsheet, activeRoi, EVerificationStatus.Unverified, "deleted-logsheet");
        var deletedTemplateValue = CreateExtractedValue(
            logsheetWithDeletedTemplate,
            deletedTemplateRoi,
            EVerificationStatus.Unverified,
            "deleted-template"
        );

        _dbContext.Templates.AddRange(activeTemplate, deletedTemplate);
        _dbContext.Rois.AddRange(activeRoi, deletedTemplateRoi);
        _dbContext.Logsheets.AddRange(activeLogsheet, deletedLogsheet, logsheetWithDeletedTemplate);
        _dbContext.ExtractedValues.AddRange(expected, verified, deletedLogsheetValue, deletedTemplateValue);
        await _dbContext.SaveChangesAsync();

        var expectedDto = new ExtractedValueDto(
            expected.Id,
            expected.LogsheetId,
            expected.RoiId,
            expected.Roi.Type,
            expected.Roi.VariableName,
            expected.Value,
            expected.CorrectedValue,
            expected.Status,
            expected.CreatedAt,
            expected.UpdatedAt
        );

        _mapperMock
            .Setup(m => m.Map<ExtractedValueDto>(It.Is<ExtractedValue>(e => e.Id == expected.Id)))
            .Returns(expectedDto);

        var result = await GetRandomUnverifiedExtractedValueHandler.Handle(
            new GetRandomUnverifiedExtractedValueQuery(),
            _dbContext,
            _mapperMock.Object,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        _mapperMock.Verify(
            m => m.Map<ExtractedValueDto>(It.Is<ExtractedValue>(e => e.Id == expected.Id)),
            Times.Once
        );
    }

    private static LogsheetXtractor.Domain.Entities.Template CreateTemplate(string name)
    {
        return new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = name,
            FileId = Guid.NewGuid(),
            File = new LogsheetXtractor.Domain.Entities.File
            {
                OriginalFileName = $"{name}.pdf",
                StoredFileName = $"{name}.pdf",
                StoragePath = $"{name}.pdf",
                ContentType = "application/pdf",
                SizeBytes = 1,
            },
        };
    }

    private static Logsheet CreateLogsheet(LogsheetXtractor.Domain.Entities.Template template, string fileName)
    {
        return new Logsheet
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            FileId = Guid.NewGuid(),
            File = new LogsheetXtractor.Domain.Entities.File
            {
                OriginalFileName = $"{fileName}.pdf",
                StoredFileName = $"{fileName}.pdf",
                StoragePath = $"{fileName}.pdf",
                ContentType = "application/pdf",
                SizeBytes = 1,
            },
            Status = ELogSheetStatus.NeedsReview,
        };
    }

    private static Roi CreateRoi(LogsheetXtractor.Domain.Entities.Template template, string variableName)
    {
        return new Roi
        {
            Id = Guid.NewGuid(),
            TemplateId = template.Id,
            Template = template,
            VariableName = variableName,
            Type = ERoiType.Number,
            Coordinates = new Coordinates(1, 2, 3, 4),
        };
    }

    private static ExtractedValue CreateExtractedValue(
        Logsheet logsheet,
        Roi roi,
        EVerificationStatus status,
        string value
    )
    {
        return new ExtractedValue
        {
            Id = Guid.NewGuid(),
            LogsheetId = logsheet.Id,
            Logsheet = logsheet,
            RoiId = roi.Id,
            Roi = roi,
            Status = status,
            Value = value,
        };
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
