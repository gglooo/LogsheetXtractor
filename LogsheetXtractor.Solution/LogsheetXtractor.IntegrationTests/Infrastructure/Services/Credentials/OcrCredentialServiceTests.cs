using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Infrastructure.Services.Credentials;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace LogsheetXtractor.IntegrationTests.Infrastructure.Services.Credentials;

public class OcrCredentialServiceTests
{
    [Fact]
    public void GetCredentialFilePath_ShouldReturnPath_WhenFileExists()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var configData = new Dictionary<string, string?>
            {
                { "Credentials:GoogleApiKeyPath", tempFile },
            };
            IConfiguration config = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();

            var service = new OcrCredentialService(config);

            var result = service.GetCredentialFilePath(ECredentialType.Google);

            result.Should().NotBeNull();
            result!.Value.Item1.Should().Be(ECredentialType.Google);
            result!.Value.Item2.Should().Be(tempFile);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public void GetCredentialFilePath_ShouldReturnNull_WhenPathDoesNotExist()
    {
        var configData = new Dictionary<string, string?>
        {
            { "Credentials:GoogleApiKeyPath", "non_existent_file.json" },
        };
        IConfiguration config = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var service = new OcrCredentialService(config);

        var result = service.GetCredentialFilePath(ECredentialType.Google);

        result.Should().BeNull();
    }

    [Fact]
    public void GetCredentialFilePath_ShouldReturnNull_WhenConfigIsMissing()
    {
        IConfiguration config = new ConfigurationBuilder().Build();
        var service = new OcrCredentialService(config);

        var result = service.GetCredentialFilePath(ECredentialType.Google);
        result.Should().BeNull();
    }
}
