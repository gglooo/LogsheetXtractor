using System.Text.Json;
using FluentAssertions;
using LogsheetXtractor.Infrastructure.Services.Scripting;
using LogsheetXtractor.Infrastructure.Services.Scripting.DTOs;
using LogsheetXtractor.Infrastructure.Services.Storage;
using LogsheetXtractor.Domain.ValueObjects;
using Moq;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Scripting;

public class ScriptOutputParserTests
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly Mock<LogsheetXtractor.Application.Interfaces.ICoordinateTransformerService> _coordinateTransformerMock = new();
    private readonly ScriptOutputParser _parser;

    public ScriptOutputParserTests()
    {
        _parser = new ScriptOutputParser(
            _fileStorageServiceMock.Object,
            _coordinateTransformerMock.Object
        );
    }

    [Fact]
    public async Task ParseProcessLogsheetCsv_ShouldReturnDictionary_WhenCsvIsValid()
    {
        var filePath = "test.csv";
        var csvContent = "Variable,Value\nVar1,Val1\nVar2,Val2";

        _fileStorageServiceMock
            .Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(csvContent);

        var result = await _parser.ParseProcessLogsheetCsvAsync(filePath);

        result.Should().HaveCount(2);
        result["Var1"].Should().Be("Val1");
        result["Var2"].Should().Be("Val2");
    }

    [Fact]
    public async Task ParseSelectRoisJson_ShouldReturnDto_WhenJsonIsValid()
    {
        var filePath = "test.json";
        var templateId = Guid.NewGuid();
        var pythonDto = new PythonSelectRoisOutputDto
        {
            Content = new List<PythonRoiDto>
            {
                new PythonRoiDto
                {
                    VarName = "roi1",
                    Coords = new List<int> { 10, 10, 20, 20 },
                },
            },
            ToIgnore = new List<PythonResidualDto>(),
        };
        var jsonContent = JsonSerializer.Serialize(pythonDto);

        _fileStorageServiceMock
            .Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonContent);

        var result = await _parser.ParseSelectRoisJsonAsync(filePath, templateId);

        result.Rois.Should().HaveCount(1);
        result.Rois.First().VariableName.Should().Be("roi1");
    }

    [Fact]
    public async Task ParseSelectRoisJson_ShouldReturnEmpty_WhenJsonIsInvalidOrEmpty()
    {
        var filePath = "test.json";
        var jsonContent = "{}";

        _fileStorageServiceMock
            .Setup(x => x.ReadAllTextAsync(filePath, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonContent);

        var result = await _parser.ParseSelectRoisJsonAsync(filePath, Guid.NewGuid());

        result.Should().NotBeNull();
        result.Rois.Should().BeEmpty();
    }

    [Fact]
    public void ParseAutomaticAlignmentJson_ShouldNormalizeFrontsideAlignment()
    {
        var rawJson = """
            {
              "frontside": {
                "templatePoints": [{ "x": 0, "y": 0 }],
                "targetPoints": [{ "x": 10, "y": 20 }],
                "imageWidth": 1000,
                "imageHeight": 2000
              }
            }
            """;
        var expectedPoints = new List<PointCoordinate> { new(1, 2) };

        _coordinateTransformerMock
            .Setup(x =>
                x.NormalizeAlignmentPoints(
                    It.IsAny<List<PointCoordinate>>(),
                    It.IsAny<List<PointCoordinate>>(),
                    100,
                    200,
                    1000,
                    2000
                )
            )
            .Returns(expectedPoints);

        var result = _parser.ParseAutomaticAlignmentJson(rawJson, 100, 200);

        result.Frontside.Should().BeEquivalentTo(expectedPoints);
        result.Backside.Should().BeNull();
    }

    [Fact]
    public void ParseAutomaticAlignmentJson_ShouldThrow_WhenCliResponseIsMalformed()
    {
        var act = () => _parser.ParseAutomaticAlignmentJson("""{"unexpected": true}""", 100, 200);

        act.Should().Throw<ArgumentException>().WithMessage("Invalid alignment JSON format.");
    }

    [Fact]
    public void ParseAutomaticAlignmentJson_ShouldThrow_WhenBacksideDimensionsAreMissing()
    {
        var rawJson = """
            {
              "frontside": {
                "templatePoints": [{ "x": 0, "y": 0 }],
                "targetPoints": [{ "x": 10, "y": 20 }],
                "imageWidth": 1000,
                "imageHeight": 2000
              },
              "backside": {
                "templatePoints": [{ "x": 0, "y": 0 }],
                "targetPoints": [{ "x": 30, "y": 40 }],
                "imageWidth": 1000,
                "imageHeight": 2000
              }
            }
            """;

        var act = () => _parser.ParseAutomaticAlignmentJson(rawJson, 100, 200);

        act.Should()
            .Throw<ArgumentException>()
            .WithMessage("Backside template dimensions must be provided if backside alignment is present.");
    }
}
