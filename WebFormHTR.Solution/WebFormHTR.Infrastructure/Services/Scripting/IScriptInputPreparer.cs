using WebFormHTR.Domain.Entities;

namespace WebFormHTR.Infrastructure.Services.Scripting;

public interface IScriptInputPreparer
{
    Task<string> CreateTemplateConfigAsync(Template template, CancellationToken ct);
    Task<string> CreateAlignmentArgumentAsync(Logsheet logsheet, CancellationToken ct);
}