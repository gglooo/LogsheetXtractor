using System.Text.Json;
using FluentAssertions;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Infrastructure.Services.Scripting;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;

namespace WebFormHTR.Tests.Infrastructure.Services.Scripting;

using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Residuals.DTOs;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects;

public class PythonHtrAdapterTests
{
    private readonly Mock<IScriptExecutor> _scriptExecutorMock;
    private readonly Mock<ICredentialService> _credentialServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IScriptInputPreparer> _inputPreparerMock;
    private readonly Mock<IScriptOutputParser> _outputParserMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<PythonHtrAdapter>> _loggerMock;
    private readonly PythonHtrAdapter _adapter;
    private readonly IMapper _mapper;

    public PythonHtrAdapterTests()
    {
        _scriptExecutorMock = new Mock<IScriptExecutor>();
        _credentialServiceMock = new Mock<ICredentialService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _configMock = new Mock<IConfiguration>();
        _inputPreparerMock = new Mock<IScriptInputPreparer>();
        _outputParserMock = new Mock<IScriptOutputParser>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<PythonHtrAdapter>>();
        _mapper = new Mock<IMapper>().Object;

        _adapter = new PythonHtrAdapter(
            _scriptExecutorMock.Object,
            _credentialServiceMock.Object,
            _fileStorageServiceMock.Object,
            _mapper,
            _inputPreparerMock.Object,
            _outputParserMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task SelectRoisAsync_ShouldExecuteScriptAndReturnRois()
    {
        var template = new Domain.Entities.Template
        {
            Id = Guid.NewGuid(),
            File = new Domain.Entities.File { StoragePath = "input.pdf" }
        };
        var input = new SelectRoisInputDto(template);
        var ct = CancellationToken.None;
        var credentialsPath = "/path/to/creds.json";
        var resolvedInputPath = "/resolved/input.pdf";
        var resolvedOutputPath = "/resolved/selected_rois.json";

        _credentialServiceMock.Setup(x => x.GetAvailableCredentialsPath())
            .Returns(new List<(ECredentialType, string)>
            {
                (ECredentialType.Google, credentialsPath)
            });

        _fileStorageServiceMock.Setup(x => x.GetResolvedPath(template.File.StoragePath))
            .Returns(resolvedInputPath);
        _fileStorageServiceMock.Setup(x => x.GetTemporaryFilePath(It.Is<string>(s => s.EndsWith("selected_rois.json"))))
            .Returns(resolvedOutputPath);

        _scriptExecutorMock.Setup(x => x.ExecuteScriptAsync(
                "select_rois.py",
                It.Is<string>(args =>
                    args.Contains($"--pdf_file {resolvedInputPath}") &&
                    args.Contains($"--output_file {resolvedOutputPath}") &&
                    args.Contains("--autodetect") &&
                    args.Contains($"--credentials {credentialsPath}")),
                ct))
            .ReturnsAsync("Script output");

        var expectedOutput = new SelectRoisOutputDto(
            new List<RoiDto>
            {
                new RoiDto
                (
                    Guid.NewGuid(),
                    "TestROI",
                    template.Id,
                    ERoiType.Handwritten,
                    new Coordinates(10, 20, 100, 50),
                    DateTime.UtcNow,
                    null
                )
            },
            new List<ResidualDto>
            {
                new ResidualDto
                (
                    Guid.NewGuid(),
                    template.Id,
                    "Ignored Content",
                    new Coordinates(200, 200, 50, 50),
                    DateTime.UtcNow,
                    null
                )
            }
        );

        _outputParserMock.Setup(x => x.ParseSelectRoisJson(resolvedOutputPath, template.Id))
            .Returns(expectedOutput);

        var result = await _adapter.SelectRoisAsync(input, ct);

        result.Should().NotBeNull();
        result.Rois.Should().HaveCount(1);
        var roi = result.Rois.First();
        roi.Coordinates.X.Should().Be(10);
        roi.Coordinates.Y.Should().Be(20);
        roi.Coordinates.Width.Should().Be(100);
        roi.Coordinates.Height.Should().Be(50);
        roi.VariableName.Should().Be("TestROI");

        result.Residuals.Should().HaveCount(1);
        var residual = result.Residuals.First();
        residual.Coordinates.X.Should().Be(200);
        residual.Coordinates.Y.Should().Be(200);
        residual.Coordinates.Width.Should().Be(50);
        residual.Coordinates.Height.Should().Be(50);
        residual.Content.Should().Be("Ignored Content");
    }

    [Fact]
    public async Task SelectRoisAsync_ShouldThrow_WhenCredentialsMissing()
    {
        var template = new Domain.Entities.Template { File = new Domain.Entities.File { StoragePath = "input.pdf" } };
        var input = new SelectRoisInputDto(template);

        _credentialServiceMock.Setup(x => x.GetAvailableCredentialsPath())
            .Returns([]);

        var act = async () => await _adapter.SelectRoisAsync(input, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("No credentials available for ROI selection.");
    }
}