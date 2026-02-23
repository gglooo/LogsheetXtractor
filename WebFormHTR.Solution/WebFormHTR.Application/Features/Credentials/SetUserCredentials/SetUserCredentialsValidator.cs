using FluentValidation;

namespace WebFormHTR.Application.Features.Credentials.SetUserCredentials;

public class SetUserCredentialsValidator : AbstractValidator<SetUserCredentialsCommand>
{
    public SetUserCredentialsValidator()
    {
        RuleFor(x => x.Keys)
            .NotEmpty().WithMessage("At least one credential must be provided.");

        RuleForEach(x => x.Keys)
            .Must(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .WithMessage("Credential value cannot be empty.");
    }
}
