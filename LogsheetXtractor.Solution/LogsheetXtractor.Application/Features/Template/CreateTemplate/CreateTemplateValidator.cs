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
    }

    private async Task<bool> BeUniqueNameAsync(string name, CancellationToken cancellationToken)
    {
        return !await _dbContext.Templates.AnyAsync(t => t.Name == name, cancellationToken);
    }
}
