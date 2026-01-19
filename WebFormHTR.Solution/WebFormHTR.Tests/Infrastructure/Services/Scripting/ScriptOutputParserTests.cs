using System.Text.Json;
using FluentAssertions;
using Moq;
using WebFormHTR.Infrastructure.Services.Scripting;
using WebFormHTR.Infrastructure.Services.Scripting.DTOs;
using WebFormHTR.Infrastructure.Services.Storage;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services.Scripting;

public class ScriptOutputParserTests
{
    private readonly Mock<IFileStorageService> _fileStorageServiceMock = new();
    private readonly ScriptOutputParser _parser;

    public ScriptOutputParserTests()
    {
        _parser = new ScriptOutputParser(_fileStorageServiceMock.Object);
    }

    [Fact]
    public void ParseProcessLogsheetCsv_ShouldReturnDictionary_WhenCsvIsValid()
    {
        var filePath = "test.csv";
        var csvContent = "Variable,Value\nVar1,Val1\nVar2,Val2";
        
        _fileStorageServiceMock.Setup(x => x.ReadAllText(filePath)).Returns(csvContent);

        var result = _parser.ParseProcessLogsheetCsv(filePath);

        result.Should().HaveCount(2);
        result["Var1"].Should().Be("Val1");
        result["Var2"].Should().Be("Val2");
    }

    [Fact]
    public void ParseSelectRoisJson_ShouldReturnDto_WhenJsonIsValid()
    {
        var filePath = "test.json";
        var templateId = Guid.NewGuid();
        var pythonDto = new PythonSelectRoisOutputDto
        {
            Content = new List<PythonRoiDto> 
            { 
                new PythonRoiDto { VarName = "roi1", Coords = new List<int> { 10, 10, 20, 20 } } 
            },
            ToIgnore = new List<PythonResidualDto>()
        };
        var jsonContent = JsonSerializer.Serialize(pythonDto);

        _fileStorageServiceMock.Setup(x => x.ReadAllText(filePath)).Returns(jsonContent);

        var result = _parser.ParseSelectRoisJson(filePath, templateId);

        result.Rois.Should().HaveCount(1);
        result.Rois.First().VariableName.Should().Be("roi1");
    }

    [Fact]
    public void ParseSelectRoisJson_ShouldReturnEmpty_WhenJsonIsInvalidOrEmpty()
    {
        var filePath = "test.json";
        var jsonContent = "{}"; 

        _fileStorageServiceMock.Setup(x => x.ReadAllText(filePath)).Returns(jsonContent);

        var result = _parser.ParseSelectRoisJson(filePath, Guid.NewGuid());
        
        result.Should().NotBeNull();
        result.Rois.Should().BeEmpty();
    }
}
