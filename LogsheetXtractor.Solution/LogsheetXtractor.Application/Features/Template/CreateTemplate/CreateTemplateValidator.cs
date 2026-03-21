using FluentValidation;
using LogsheetXtractor.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LogsheetXtractor.Application.Features.Template.CreateTemplate;

public class CreateTemplateValidator : AbstractValidator<CreateTemplateCommand>
{
    private readonly IAppDbContext _dbContext;

    public CreateTemplateValidator(IAppDbContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .MustAsync(BeUniqueNameAsync)
            .WithMessage("A template with this name already exists.");

        RuleFor(x => x.Backside)
            .MustAsync(
                async (command, backside, cancellationToken) =>
                {
                    if (backside is null)
                    {
                        return true;
                    }

                    return await BeUniqueNameAsync(backside.Name, cancellationToken);
                }
            )
            .WithMessage("A template with the backside name already exists.");

        RuleFor(x => x)
            .Must(command => command.Name != command.Backside?.Name)
            .WithMessage("The template name and backside name cannot be the same.");
        ;
    }

    private async Task<bool> BeUniqueNameAsync(string name, CancellationToken cancellationToken)
    {
        return !await _dbContext.Templates.AnyAsync(t => t.Name == name, cancellationToken);
    }
}
