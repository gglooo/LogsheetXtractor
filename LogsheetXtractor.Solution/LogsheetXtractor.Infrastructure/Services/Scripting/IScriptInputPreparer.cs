using LogsheetXtractor.Domain.Entities;

namespace LogsheetXtractor.Infrastructure.Services.Scripting;

public interface IScriptInputPreparer
{
    Task<string> CreateTemplateConfigAsync(Template template, CancellationToken ct);
    Task<PreparedAlignmentInput> PrepareAlignmentInputAsync(
        Logsheet logsheet,
        bool hasBacksidePage,
        CancellationToken ct
    );
    Task<PreparedBacksideInput?> PrepareBacksideInputAsync(
        Logsheet logsheet,
        bool hasBacksidePage,
        CancellationToken ct
    );
}
