using FluentAssertions;
using FluentResults;
using LogsheetXtractor.Application.Errors;
using LogsheetXtractor.Application.Features.ExtractedValues;
using LogsheetXtractor.Infrastructure.Persistence;
using LogsheetXtractor.UnitTests.Common;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LogsheetXtractor.UnitTests.Application.Features.ExtractedValues;

public class BatchVerifyExtractedValuesTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly IMapper _mapper;

    public BatchVerifyExtractedValuesTests()
    {
        _dbContext = TestDbContextFactory.Create();
        _mapper = new Mapper();
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }

    [Fact]
    public async Task Handle_ShouldVerifyExtractedValues_WhenValidIdsProvided()
    {
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Test",
        };
        var roi = new LogsheetXtractor.Domain.Entities.Roi
        {
            Id = Guid.NewGuid(),
            Template = template,
            VariableName = "TestVar",
        };
        var logsheet = new LogsheetXtractor.Domain.Entities.Logsheet
        {
            Id = Guid.NewGuid(),
            Status = LogsheetXtractor.Domain.Enums.ELogSheetStatus.NeedsReview,
            Template = template,
        };
        var ev1 = new LogsheetXtractor.Domain.Entities.ExtractedValue
        {
            Id = Guid.NewGuid(),
            Logsheet = logsheet,
            Roi = roi,
            Value = "test1",
            Status = LogsheetXtractor.Domain.Enums.EVerificationStatus.Unverified,
        };
        var ev2 = new LogsheetXtractor.Domain.Entities.ExtractedValue
        {
            Id = Guid.NewGuid(),
            Logsheet = logsheet,
            Roi = roi,
            Value = "test2",
            Status = LogsheetXtractor.Domain.Enums.EVerificationStatus.Unverified,
        };

        _dbContext.Templates.Add(template);
        _dbContext.Rois.Add(roi);

        _dbContext.Logsheets.Add(logsheet);
        _dbContext.ExtractedValues.AddRange(ev1, ev2);
        await _dbContext.SaveChangesAsync();

        var command = new BatchVerifyExtractedValuesCommand(new[] { ev1.Id, ev2.Id });

        var result = await BatchVerifyExtractedValuesHandler.Handle(
            command,
            _dbContext,
            _mapper,
            CancellationToken.None
        );

        result.IsSuccess.Should().BeTrue();

        _dbContext.ChangeTracker.Clear();

        var verifiedEv1 = await _dbContext.ExtractedValues.FindAsync(ev1.Id);
        var verifiedEv2 = await _dbContext.ExtractedValues.FindAsync(ev2.Id);

        verifiedEv1!.Status.Should().Be(LogsheetXtractor.Domain.Enums.EVerificationStatus.Verified);
        verifiedEv2!.Status.Should().Be(LogsheetXtractor.Domain.Enums.EVerificationStatus.Verified);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLogsheetIsCompleted()
    {
        var template = new LogsheetXtractor.Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            Name = "Test",
        };
        var roi = new LogsheetXtractor.Domain.Entities.Roi
        {
            Id = Guid.NewGuid(),
            Template = template,
            VariableName = "TestVar",
        };
        var logsheet = new LogsheetXtractor.Domain.Entities.Logsheet
        {
            Id = Guid.NewGuid(),
            Status = LogsheetXtractor.Domain.Enums.ELogSheetStatus.Completed,
            Template = template,
        };
        var ev1 = new LogsheetXtractor.Domain.Entities.ExtractedValue
        {
            Id = Guid.NewGuid(),
            Logsheet = logsheet,
            Roi = roi,
            Value = "test1",
            Status = LogsheetXtractor.Domain.Enums.EVerificationStatus.Verified,
        };

        _dbContext.Templates.Add(template);
        _dbContext.Rois.Add(roi);
        _dbContext.Logsheets.Add(logsheet);
        _dbContext.ExtractedValues.Add(ev1);
        await _dbContext.SaveChangesAsync();

        _dbContext.ChangeTracker.Clear();

        var command = new BatchVerifyExtractedValuesCommand(new[] { ev1.Id });

        var result = await BatchVerifyExtractedValuesHandler.Handle(
            command,
            _dbContext,
            _mapper,
            CancellationToken.None
        );

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainItemsAssignableTo<InvalidStateError>();
    }
}
