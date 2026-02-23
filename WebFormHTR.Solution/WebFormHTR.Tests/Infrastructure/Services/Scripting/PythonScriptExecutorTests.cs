using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using WebFormHTR.Infrastructure.Services.Scripting;
using Xunit;

namespace WebFormHTR.Tests.Infrastructure.Services.Scripting;

public class PythonScriptExecutorTests
{
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<Microsoft.Extensions.Logging.ILogger<PythonScriptExecutor>> _loggerMock = new();
    private readonly Mock<PythonScriptExecutor> _serviceMock;

    public PythonScriptExecutorTests()
    {
        _configMock.Setup(c => c["Python:InterpreterPath"]).Returns("python3");
        _configMock.Setup(c => c["Python:ScriptsFolder"]).Returns("./");

        _serviceMock = new Mock<PythonScriptExecutor>(_configMock.Object, _loggerMock.Object) { CallBase = true };
    }

    [Fact]
    public async Task ExecuteScriptWithJsonOutputAsync_ShouldEnsureDeserialization_WhenOutputIsValid()
    {
        var scriptName = "test_script.py";
        var args = new[] { "--test" };
        var expectedResult = new TestDto { Message = "Success" };
        var jsonOutput = JsonSerializer.Serialize(expectedResult);

        _serviceMock.Setup(x => x.ExecuteScriptAsync(scriptName, args, It.IsAny<CancellationToken>()))
            .ReturnsAsync(jsonOutput);

        var result = await _serviceMock.Object.ExecuteScriptWithJsonOutputAsync<TestDto>(scriptName, args, CancellationToken.None);

        result.Should().NotBeNull();
        result.Message.Should().Be("Success");
    }

    [Fact]
    public async Task ExecuteScriptWithJsonOutputAsync_ShouldThrow_WhenOutputIsInvalidJson()
    {
        var scriptName = "test_script.py";
        var args = new[] { "--test" };
        var invalidJson = "Not JSON";

        _serviceMock.Setup(x => x.ExecuteScriptAsync(scriptName, args, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invalidJson);

        Func<Task> act = async () => await _serviceMock.Object.ExecuteScriptWithJsonOutputAsync<TestDto>(scriptName, args, CancellationToken.None);

        await act.Should().ThrowAsync<JsonException>(); 
    }

    public class TestDto
    {
        public string Message { get; set; } = string.Empty;
    }
}
