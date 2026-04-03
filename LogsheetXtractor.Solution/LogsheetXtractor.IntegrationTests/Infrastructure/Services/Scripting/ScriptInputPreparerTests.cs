using System.Text;
using System.Text.Json;
using FluentAssertions;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.ValueObjects;
using LogsheetXtractor.Infrastructure.Services.Scripting;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using LogsheetXtractor.Infrastructure.Services.Storage;
using MapsterMapper;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Scripting;

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
            Rois = new List<PythonRoiDto>(),
        };
        var expectedPath = "path/to/config.json";

        _mapperMock.Setup(x => x.Map<PythonTemplateConfig>(template)).Returns(pythonConfig);

        _fileStorageServiceMock
            .Setup(x =>
                x.SaveTemporaryFileAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedPath);

        var result = await _preparer.CreateTemplateConfigAsync(template, CancellationToken.None);

        result.Should().Be(expectedPath);
        _fileStorageServiceMock.Verify(
            x =>
                x.SaveTemporaryFileAsync(
                    It.Is<byte[]>(b =>
                        JsonSerializer.Deserialize<PythonTemplateConfig>(
                            Encoding.UTF8.GetString(b),
                            (JsonSerializerOptions?)null
                        ) != null
                    ),
                    It.Is<string>(s => s.EndsWith(".json")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldReturnAlignedFlag_WhenNoFrontsidePoints()
    {
        var logsheet = new Logsheet
        {
            Template = new Template { Width = 100, Height = 100 },
            AlignmentData = new AlignmentContainer(new List<PointCoordinate>(), null),
        };

        var result = await _preparer.CreateAlignmentArgumentAsync(
            logsheet,
            true,
            CancellationToken.None
        );

        result.Should().BeEquivalentTo(new[] { "--aligned" });
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldSaveAlignmentConfig_WhenPointsExist()
    {
        var logsheet = new Logsheet
        {
            Template = new Template { Width = 100, Height = 100 },
            AlignmentData = new AlignmentContainer(new List<PointCoordinate> { new(1, 1) }, null),
        };
        var expectedPath = "path/to/alignment.json";

        _fileStorageServiceMock
            .Setup(x =>
                x.SaveTemporaryFileAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedPath);

        var result = await _preparer.CreateAlignmentArgumentAsync(
            logsheet,
            true,
            CancellationToken.None
        );

        result.Should().BeEquivalentTo(new[] { "--alignment_config", expectedPath });
        _fileStorageServiceMock.Verify(
            x =>
                x.SaveTemporaryFileAsync(
                    It.IsAny<byte[]>(),
                    It.Is<string>(s => s.Contains("alignment_config.json")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldThrow_WhenTemplateDimensionsMissing()
    {
        var logsheet = new Logsheet
        {
            Template = new Template { Width = 0, Height = 0 },
            AlignmentData = new AlignmentContainer(new List<PointCoordinate> { new(1, 1) }, null),
        };

        Func<Task> act = async () =>
            await _preparer.CreateAlignmentArgumentAsync(logsheet, true, CancellationToken.None);

        await act.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("Template dimensions are required for alignment configuration.");
    }

    [Fact]
    public async Task CreateAlignmentArgumentAsync_ShouldSaveBothConfigs_WhenOnlyBacksidePointsExist()
    {
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Width = 100,
            Height = 100,
        };
        var backsideTemplate = new Template
        {
            Id = Guid.NewGuid(),
            Width = 100,
            Height = 100,
        };
        template.ForceSetBacksideTemplate(backsideTemplate);

        var logsheet = new Logsheet
        {
            Template = template,
            AlignmentData = new AlignmentContainer(null, new List<PointCoordinate> { new(1, 1) }),
        };
        var expectedFrontPath = "path/to/front_alignment.json";
        var expectedBackPath = "path/to/back_alignment.json";

        _fileStorageServiceMock
            .SetupSequence(x =>
                x.SaveTemporaryFileAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(expectedFrontPath)
            .ReturnsAsync(expectedBackPath);

        var result = await _preparer.CreateAlignmentArgumentAsync(
            logsheet,
            true,
            CancellationToken.None
        );

        result.Should().ContainInOrder("--alignment_config", expectedFrontPath);
        result.Should().ContainInOrder("--backside_alignment_config", expectedBackPath);

        _fileStorageServiceMock.Verify(
            x =>
                x.SaveTemporaryFileAsync(
                    It.IsAny<byte[]>(),
                    It.Is<string>(s => s.Contains("alignment_config.json")),
                    It.IsAny<CancellationToken>()
                ),
            Times.Exactly(2)
        );
    }
}
