using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WebFormHTR.Application.Interfaces;

namespace WebFormHTR.Application.Features.Template.CreateTemplate;

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
            .MustAsync(async (command, backside, cancellationToken) =>
            {
                if (backside is null)
                {
                    return true;
                }

                return await BeUniqueNameAsync(backside.Name, cancellationToken);
            })
            .WithMessage("A template with the backside name already exists.");
    }

    private async Task<bool> BeUniqueNameAsync(string name, CancellationToken cancellationToken)
    {
        return !await _dbContext.Templates.AnyAsync(t => t.Name == name, cancellationToken);
    }
}