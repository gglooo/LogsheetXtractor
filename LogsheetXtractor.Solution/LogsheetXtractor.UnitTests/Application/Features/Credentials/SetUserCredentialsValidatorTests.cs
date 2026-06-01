using FluentAssertions;
using LogsheetXtractor.Application.Features.Credentials;
using LogsheetXtractor.Application.Features.Credentials.SetUserCredentials;

namespace LogsheetXtractor.UnitTests.Application.Features.Credentials;

public class SetUserCredentialsValidatorTests
{
    private readonly SetUserCredentialsValidator _validator = new();

    [Fact]
    public void Validate_ShouldFail_WhenKeysAreNull()
    {
        var command = new SetUserCredentialsCommand(null!, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Credentials cannot be null.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenAllKeysAreBlank()
    {
        var command = new SetUserCredentialsCommand(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = " ",
                [ECredentialType.Azure] = "",
            },
            null
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.ErrorMessage == "At least one credential must be provided."
        );
    }

    [Fact]
    public void Validate_ShouldSucceed_WhenAtLeastOneKeyHasValue()
    {
        var command = new SetUserCredentialsCommand(
            new Dictionary<ECredentialType, string>
            {
                [ECredentialType.Google] = "google-key",
                [ECredentialType.Azure] = "",
            },
            null
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
