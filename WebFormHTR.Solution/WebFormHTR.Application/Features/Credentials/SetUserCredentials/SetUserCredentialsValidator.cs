using FluentValidation;

namespace WebFormHTR.Application.Features.Credentials.SetUserCredentials;

public class SetUserCredentialsValidator : AbstractValidator<SetUserCredentialsCommand>
{
    public SetUserCredentialsValidator()
    {
        RuleFor(x => x.Keys)
            .NotNull()
            .WithMessage("Credentials cannot be null.")
            .Must(keys => keys.Any(kvp => !string.IsNullOrWhiteSpace(kvp.Value)))
            .WithMessage("At least one credential must be provided.");
    }
}
