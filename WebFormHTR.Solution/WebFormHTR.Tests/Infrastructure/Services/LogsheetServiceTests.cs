using FluentAssertions;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using WebFormHTR.Application.Features.Logsheets.DTOs;
using WebFormHTR.Application.Features.Scripting;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Infrastructure.Services;
using WebFormHTR.Application.Features.ExtractedValues.DTOs;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services;

public class LogsheetServiceTests
{
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly Mock<IHtrScriptEngine> _scriptEngineMock = new();
    private readonly LogsheetService _service;

    public LogsheetServiceTests()
    {
        _service = new LogsheetService(_mapperMock.Object, _scriptEngineMock.Object);
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldProcess_WhenStateIsValid()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Pending };
        var processOutput = new ProcessLogsheetOutputDto(new Dictionary<string, string>());
        
        _scriptEngineMock.Setup(x => x.ProcessLogsheetAsync(It.IsAny<ProcessLogsheetInputDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(processOutput);

        var expectedDto = new LogsheetDetailDto(
            logsheet.Id,
            null!,
            null,
            null!,
            ELogSheetStatus.NeedsReview,
            null,
            null,
            new List<WebFormHTR.Application.Features.ExtractedValues.DTOs.ExtractedValueDto>(),
            DateTime.UtcNow,
            null
        );

        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(logsheet))
            .Returns(expectedDto);

        var result = await _service.ProcessLogsheetAsync(logsheet, CancellationToken.None);

        result.Should().Be(expectedDto);
        logsheet.Status.Should().Be(ELogSheetStatus.NeedsReview);
        logsheet.ProcessedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldThrow_WhenLogsheetIsNull()
    {
        Func<Task> act = async () => await _service.ProcessLogsheetAsync(null!, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Logsheet not found");
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldThrow_WhenStatusIsInvalid()
    {
        var logsheet = new Logsheet { Status = ELogSheetStatus.Completed };
        Func<Task> act = async () => await _service.ProcessLogsheetAsync(logsheet, CancellationToken.None);
        await act.Should().ThrowAsync<ValidationException>().WithMessage("Logsheet is not in a valid state for processing");
    }

    [Fact]
    public async Task ProcessLogsheetAsync_ShouldSetFailedStatus_WhenEngineThrows()
    {
        var logsheet = new Logsheet { Id = Guid.NewGuid(), Status = ELogSheetStatus.Pending };
        var errorMessage = "Engine failure";
        
        _scriptEngineMock.Setup(x => x.ProcessLogsheetAsync(It.IsAny<ProcessLogsheetInputDto>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

         
        var expectedDto = new LogsheetDetailDto(
            logsheet.Id, null!, null, null!, ELogSheetStatus.Failed, null, null, new List<WebFormHTR.Application.Features.ExtractedValues.DTOs.ExtractedValueDto>(), DateTime.UtcNow, null
        );
        _mapperMock.Setup(x => x.Map<LogsheetDetailDto>(logsheet)).Returns(expectedDto);

        var result = await _service.ProcessLogsheetAsync(logsheet, CancellationToken.None);
        
        logsheet.Status.Should().Be(ELogSheetStatus.Failed);
        logsheet.ErrorMessage.Should().Be(errorMessage);
    }
}
