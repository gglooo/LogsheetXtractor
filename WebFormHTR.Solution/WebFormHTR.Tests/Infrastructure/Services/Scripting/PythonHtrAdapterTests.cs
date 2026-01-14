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

public class PythonHtrAdapterTests
{
    private readonly Mock<IScriptExecutor> _scriptExecutorMock;
    private readonly Mock<ICredentialService> _credentialServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<IConfiguration> _configMock;
    private readonly PythonHtrAdapter _adapter;
    private readonly IMapper _mapper;

    public PythonHtrAdapterTests()
    {
        _scriptExecutorMock = new Mock<IScriptExecutor>();
        _credentialServiceMock = new Mock<ICredentialService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _configMock = new Mock<IConfiguration>();
        _mapper = new Mock<IMapper>().Object;

        _adapter = new PythonHtrAdapter(
            _scriptExecutorMock.Object,
            _credentialServiceMock.Object,
            _fileStorageServiceMock.Object,
            _mapper);
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

        var pythonOutput = new PythonSelectRoisOutputDto
        {
            Content = new List<PythonRoiDto>
            {
                new()
                {
                    Coords = new List<int> { 10, 20, 110, 70 }, // x, y, x2, y2 -> w=100, h=50
                    VarName = "TestROI",
                    Type = "Text"
                }
            },
            ToIgnore = new List<PythonResidualDto>
            {
                new()
                {
                    Coords = new List<int> { 200, 200, 250, 250 }, // x, y, x2, y2 -> w=50, h=50
                    Content = "Ignored Content"
                }
            }
        };
        var jsonOutput = JsonSerializer.Serialize(pythonOutput);

        _fileStorageServiceMock.Setup(x => x.ReadAllText(resolvedOutputPath))
            .Returns(jsonOutput);

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