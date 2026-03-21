using System.Linq.Expressions;
using LogsheetXtractor.Domain.Entities;
using LogsheetXtractor.Domain.Enums;

namespace LogsheetXtractor.Application.Rules;

public static class TemplateRules
{
    public static Expression<Func<Template, bool>> IsEditable =>
        template =>
            !template.Logsheets.Any(ls =>
                ls.Status == ELogSheetStatus.NeedsReview
                || ls.Status == ELogSheetStatus.Completed
                || ls.Status == ELogSheetStatus.Processing
            )
            && (
                template.FrontsideTemplate == null
                || !template.FrontsideTemplate.Logsheets.Any(ls =>
                    ls.Status == ELogSheetStatus.Completed
                    || ls.Status == ELogSheetStatus.NeedsReview
                    || ls.Status == ELogSheetStatus.Processing
                )
            );
}
