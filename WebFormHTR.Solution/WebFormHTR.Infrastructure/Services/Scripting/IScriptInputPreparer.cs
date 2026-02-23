using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public interface IScriptInputPreparer
{
    Task<string> CreateTemplateConfigAsync(Template template, CancellationToken ct);
    Task<IEnumerable<string>> CreateAlignmentArgumentAsync(Logsheet logsheet, bool hasBacksidePage, CancellationToken ct);
    Task<IEnumerable<string>> CreateBacksideArgumentAsync(Logsheet logsheet, bool hasBacksidePage, CancellationToken ct);
}