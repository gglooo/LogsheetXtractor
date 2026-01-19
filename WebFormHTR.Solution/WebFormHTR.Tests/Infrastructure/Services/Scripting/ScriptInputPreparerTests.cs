using System.Text.Json;
using System.Text;
using FluentAssertions;
using MapsterMapper;
using Moq;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.ValueObjects;
using WebFormHTR.Infrastructure.Services.Scripting;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services.Scripting;

public class ScriptInputPreparerTests
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly ScriptInputPreparer _preparer;

    public ScriptInputPreparerTests()
    {
        _preparer = new ScriptInputPreparer(_fileStorageServiceMock.Object, _mapperMock.Object);
    }

    [Fact]
    public async Task CreateTemplateConfigAsync_ShouldSaveConfigAndReturnPath()
    {
        var template = new Template { Id = Guid.NewGuid() };
        var pythonConfig = new PythonTemplateConfig 
        { 
            Width = 100, 
            Height = 100, 
            Rois = new List<PythonRoiDto>() 
        };
        var expectedPath = "path/to/config.json";

        _mapperMock.Setup(x => x.Map<PythonTemplateConfig>(template))
            .Returns(pythonConfig);

        _fileStorageServiceMock.Setup(x => x.SaveTemporaryFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPath);

        var result = await _preparer.CreateTemplateConfigAsync(template, CancellationToken.None);

        result.Should().Be(expectedPath);
        _fileStorageServiceMock.Verify(x => x.SaveTemporaryFileAsync(
            It.Is<byte[]>(b => JsonSerializer.Deserialize<PythonTemplateConfig>(Encoding.UTF8.GetString(b), (JsonSerializerOptions?)null) != null), 
            It.Is<string>(s => s.EndsWith(".json")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldReturnAlignedFlag_WhenNoFrontsidePoints()
    {
        var logsheet = new Logsheet 
        { 
            AlignmentDataModelConfig = new AlignmentContainer { Frontside = new AlignmentSideData { TargetPoints = new List<PointCoordinate>() } } 
        };

        var result = await _preparer.CreateAlignmentArgumentAsync(logsheet, CancellationToken.None);

        result.Should().Be("--aligned");
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldSaveAlignmentConfig_WhenPointsExist()
    {
        var logsheet = new Logsheet 
        { 
            Template = new Template { Width = 100, Height = 100 },
            AlignmentDataModelConfig = new AlignmentContainer 
            { 
                Frontside = new AlignmentSideData { TargetPoints = new List<PointCoordinate> { new PointCoordinate { X = 1, Y = 1 } } } 
            }
        };
        var expectedPath = "path/to/alignment.json";

        _fileStorageServiceMock.Setup(x => x.SaveTemporaryFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPath);

        var result = await _preparer.CreateAlignmentArgumentAsync(logsheet, CancellationToken.None);

        result.Should().Be($"--alignment_config {expectedPath}");
        _fileStorageServiceMock.Verify(x => x.SaveTemporaryFileAsync(
            It.IsAny<byte[]>(), 
            It.Is<string>(s => s.Contains("alignment_config.json")), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldThrow_WhenTemplateDimensionsMissing()
    {
        var logsheet = new Logsheet 
        { 
            Template = new Template { Width = 0, Height = 0 },
            AlignmentDataModelConfig = new AlignmentContainer 
            { 
                Frontside = new AlignmentSideData { TargetPoints = new List<PointCoordinate> { new PointCoordinate { X = 1, Y = 1 } } } 
            }
        };

        Func<Task> act = async () => await _preparer.CreateAlignmentArgumentAsync(logsheet, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Template dimensions are required for alignment configuration.");
    }
}
