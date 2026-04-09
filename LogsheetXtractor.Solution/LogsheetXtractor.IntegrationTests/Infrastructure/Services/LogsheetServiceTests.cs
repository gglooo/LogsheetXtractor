using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using LogsheetXtractor.Application.Features.ExtractedValues.DTOs;
using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Application.Features.RoiValidation;
using LogsheetXtractor.Application.Features.RoiValidation.DTOs;
using LogsheetXtractor.Application.Features.Scripting;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;
using LogsheetXtractor.Infrastructure.Services;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services;

public class LogsheetServiceTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock = new();
    private readonly Mock<IRoiValidationConditionEvaluator> _conditionEvaluatorMock = new();
    private readonly Mock<IRoiValidationRuleCatalogProvider> _catalogProviderMock = new();
    private readonly Mock<ILogger<LogsheetService>> _loggerMock = new();
    private readonly LogsheetService _service;

    public LogsheetServiceTests()
    {
        _conditionEvaluatorMock
            .Setup(x =>
                x.Evaluate(
                    It.IsAny<ERoiType>(),
                    It.IsAny<string?>(),
                    It.IsAny<LogsheetXtractor.Domain.ValueObjects.RoiValidation.RoiValidationConditionNode?>()
                )
            )
            .Returns([]);

        _catalogProviderMock
            .Setup(x => x.GetCatalog())
            .Returns(new RoiValidationRuleCatalogDto("v1", []));

        _service = new LogsheetService(
            _mapperMock.Object,
            _scriptEngineMock.Object,
            _conditionEvaluatorMock.Object,
            _catalogProviderMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task AlignLogsheetAsync_ShouldAlign_WhenLogsheetExists()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Pending };
        var expectedDto = new LogsheetDetailDto(
            logsheet.Id,
            null!,
            null!,
            ELogSheetStatus.Pending,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            null
        );

        _scriptEngineMock
            .Setup(x =>
                x.AutomaticAlignAsync(
                    It.Is<AutomaticAlignmentInputDto>(i => i.Logsheet.Id == logsheet.Id),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedDto);

        var result = await _service.AlignLogsheetAsync(logsheet, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
    }

    [Fact]
    public async Task AlignLogsheetAsync_ShouldReturnFail_WhenScriptEngineThrowsException()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Pending };
        var errorMessage = "Script engine failure";

        _scriptEngineMock
            .Setup(x =>
                x.AutomaticAlignAsync(
                    It.IsAny<AutomaticAlignmentInputDto>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception(errorMessage));

        var result = await _service.AlignLogsheetAsync(logsheet, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be($"Failed to align logsheet: {errorMessage}");
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldProcess_WhenStateIsValid()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Processing };
        var processOutput = new ProcessLogsheetOutputDto(new Dictionary<string, string>());

        _scriptEngineMock
            .Setup(x =>
                x.ProcessLogsheetAsync(
                    It.Is<ProcessLogsheetInputDto>(dto =>
                        dto.Options != null && dto.Options.UglyCheckboxes == true
                    ),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(processOutput);

        var expectedDto = new LogsheetDetailDto(
            logsheet.Id,
            null!,
            null!,
            ELogSheetStatus.NeedsReview,
            null,
            null,
            new List<ExtractedValueDto>(),
            DateTime.UtcNow,
            null
        );

        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(logsheet)).Returns(expectedDto);

        var options =
            new LogsheetXtractor.Application.Features.Logsheets.ProcessLogsheetDataOptions(true);
        var result = await _service.ProcessLogsheetAsync(logsheet, options, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedDto);
        logsheet.Status.Should().Be(ELogSheetStatus.NeedsReview);
        logsheet.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldReturnFail_WhenLogsheetIsNull()
    {
        var result = await _service.ProcessLogsheetAsync(null!, null, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
        result.Errors.First().Message.Should().Be("Logsheet not found");
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldReturnFail_WhenStatusIsInvalid()
    {
        var logsheet = new Logsheet { Status = ELogSheetStatus.Completed };
        var result = await _service.ProcessLogsheetAsync(logsheet, null, CancellationToken.None);
        result.IsFailed.Should().BeTrue();
        result
            .Errors.First()
            .Message.Should()
            .Be("Logsheet is not in a valid state for processing");
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldSetFailedStatus_WhenEngineFails()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Processing };
        var errorMessage = "Engine failure";

        _scriptEngineMock
            .Setup(x =>
                x.ProcessLogsheetAsync(
                    It.IsAny<ProcessLogsheetInputDto>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                FluentResults.Result.Fail(
                    new LogsheetXtractor.Application.Errors.InvalidStateError(errorMessage)
                )
            );

        var result = await _service.ProcessLogsheetAsync(logsheet, null, CancellationToken.None);

        result.IsFailed.Should().BeTrue();
        logsheet.Status.Should().Be(ELogSheetStatus.Failed);
        logsheet.ErrorMessage.Should().Be(errorMessage);
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldThrow_WhenEngineThrowsException()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Processing };
        var errorMessage = "Python HTR crashed";

        _scriptEngineMock
            .Setup(x =>
                x.ProcessLogsheetAsync(
                    It.IsAny<ProcessLogsheetInputDto>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ThrowsAsync(new Exception(errorMessage));

        var action = () => _service.ProcessLogsheetAsync(logsheet, null, CancellationToken.None);

        await action.Should().ThrowAsync<Exception>().WithMessage(errorMessage);

        logsheet.Status.Should().Be(ELogSheetStatus.Processing);
        logsheet.ErrorMessage.Should().BeNull();
    }
}
