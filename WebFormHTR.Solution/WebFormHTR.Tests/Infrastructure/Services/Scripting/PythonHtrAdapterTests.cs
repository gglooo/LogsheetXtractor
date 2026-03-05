using FluentAssertions;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Moq;
using WebFormHTR.Application.Features.Credentials;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Infrastructure.Services.Scripting;
using WebFormHTR.Infrastructure.Services.Storage;
using WebFormHTR.Infrastructure.Services.Credentials;
using WebFormHTR.Application.Features.File.Interfaces;
using WebFormHTR.Application.Features.PdfCropper;

namespace WebFormHTR.Tests.Infrastructure.Services.Scripting;

using WebFormHTR.Application.Features.ROIs.DTOs;
using WebFormHTR.Application.Features.Residuals.DTOs;
using Domain.Enums;
using Domain.ValueObjects;

public class PythonHtrAdapterTests
{
    private readonly Mock<IScriptExecutor> _scriptExecutorMock;
    private readonly Mock<ICredentialContextProvider> _credentialContextProviderMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly Mock<IScriptInputPreparer> _inputPreparerMock;
    private readonly Mock<IScriptOutputParser> _outputParserMock;
    private readonly Mock<IPdfCropperService> _pdfCropperServiceMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<PythonHtrAdapter>> _loggerMock;
    private readonly PythonHtrAdapter _adapter;
    private readonly IMapper _mapper;

    public PythonHtrAdapterTests()
    {
        _scriptExecutorMock = new Mock<IScriptExecutor>();
        _credentialContextProviderMock = new Mock<ICredentialContextProvider>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _configMock = new Mock<IConfiguration>();
        _inputPreparerMock = new Mock<IScriptInputPreparer>();
        _outputParserMock = new Mock<IScriptOutputParser>();
        _pdfCropperServiceMock = new Mock<IPdfCropperService>();
        _fileServiceMock = new Mock<IFileService>();
        _loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<PythonHtrAdapter>>();
        _mapper = new Mock<IMapper>().Object;

        _adapter = new PythonHtrAdapter(
            _scriptExecutorMock.Object,
            _credentialContextProviderMock.Object,
            _fileStorageServiceMock.Object,
            _mapper,
            _inputPreparerMock.Object,
            _outputParserMock.Object,
            _pdfCropperServiceMock.Object,
            _fileServiceMock.Object,
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

        var contextMock = new Mock<ICredentialContext>();
        contextMock.Setup(c => c.CredentialPaths)
            .Returns(new List<(ECredentialType, string)> { (ECredentialType.Google, credentialsPath) });

        _credentialContextProviderMock.Setup(x => x.GetCredentialContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contextMock.Object);

        _fileStorageServiceMock.Setup(x => x.GetResolvedPath(template.File.StoragePath))
            .Returns(resolvedInputPath);
        _fileStorageServiceMock.Setup(x => x.GetTemporaryFilePath(It.Is<string>(s => s.EndsWith("selected_rois.json"))))
            .Returns(resolvedOutputPath);

        _scriptExecutorMock.Setup(x => x.ExecuteScriptAsync(
                "select_rois.py",
                It.Is<IEnumerable<string>>(args =>
                    args.Contains("--pdf_file") && args.Contains(resolvedInputPath) &&
                    args.Contains("--output_file") && args.Contains(resolvedOutputPath) &&
                    args.Contains("--autodetect") &&
                    args.Contains("--credentials") && args.Contains(credentialsPath)),
                ct))
            .ReturnsAsync("Script output");

        var expectedOutput = new SelectRoisOutputDto(
            new List<RoiDto>
            {
                new(
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
                new(
                    Guid.NewGuid(),
                    template.Id,
                    "Ignored Content",
                    new Coordinates(200, 200, 50, 50),
                    DateTime.UtcNow,
                    null
                )
            }
        );

        _outputParserMock.Setup(x =>
                x.ParseSelectRoisJsonAsync(resolvedOutputPath, template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOutput);

        var result = await _adapter.SelectRoisAsync(input, ct);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Rois.Should().HaveCount(1);
        var roi = result.Value.Rois.First();
        roi.Coordinates.X.Should().Be(10);
        roi.Coordinates.Y.Should().Be(20);
        roi.Coordinates.Width.Should().Be(100);
        roi.Coordinates.Height.Should().Be(50);
        roi.VariableName.Should().Be("TestROI");

        result.Value.Residuals.Should().HaveCount(1);
        var residual = result.Value.Residuals.First();
        residual.Coordinates.X.Should().Be(200);
        residual.Coordinates.Y.Should().Be(200);
        residual.Coordinates.Width.Should().Be(50);
        residual.Coordinates.Height.Should().Be(50);
        residual.Content.Should().Be("Ignored Content");
    }

    [Fact]
    public async Task SelectRoisAsync_ShouldNotDetectResiduals_WhenCredentialsMissing()
    {
        var template = new Domain.Entities.Template { File = new Domain.Entities.File { StoragePath = "input.pdf" } };
        var input = new SelectRoisInputDto(template);
        var resolvedInputPath = "/resolved/input.pdf";
        var resolvedOutputPath = "/resolved/selected_rois.json";

        var contextMock = new Mock<ICredentialContext>();
        contextMock.Setup(c => c.CredentialPaths)
            .Returns(new List<(ECredentialType, string)>());

        _credentialContextProviderMock.Setup(x => x.GetCredentialContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(contextMock.Object);

        _fileStorageServiceMock.Setup(x => x.GetResolvedPath(template.File.StoragePath))
            .Returns(resolvedInputPath);
        _fileStorageServiceMock.Setup(x => x.GetTemporaryFilePath(It.Is<string>(s => s.EndsWith("selected_rois.json"))))
            .Returns(resolvedOutputPath);

        var expectedOutput = new SelectRoisOutputDto(
            new List<RoiDto>(),
            new List<ResidualDto>()
        );

        _outputParserMock.Setup(x =>
                x.ParseSelectRoisJsonAsync(resolvedOutputPath, template.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOutput);

        var result = await _adapter.SelectRoisAsync(input, CancellationToken.None);

        result.IsFailed.Should().BeFalse();
        _scriptExecutorMock.Verify(x => x.ExecuteScriptAsync(
                "select_rois.py",
                It.Is<IEnumerable<string>>(args =>
                    args.Contains("--pdf_file") && args.Contains(resolvedInputPath) &&
                    args.Contains("--output_file") && args.Contains(resolvedOutputPath) &&
                    args.Contains("--autodetect") &&
                    args.Contains("--headless") &&
                    !args.Contains("--credentials") && !args.Contains("--detect_residuals")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}