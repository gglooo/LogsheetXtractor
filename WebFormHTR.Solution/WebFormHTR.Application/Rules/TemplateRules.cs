using System.Linq.Expressions;
using WebFormHTR.Domain.Entities;
using WebFormHTR.Domain.Enums;

public static class TemplateRules
{
    public static Expression<Func<Template, bool>> IsEditable =>
        template => !template.Logsheets.Any(ls =>
            ls.Status == ELogSheetStatus.Completed ||
            ls.Status == ELogSheetStatus.NeedsReview) && !template.BacksideLogsheets.Any(ls =>
            ls.Status == ELogSheetStatus.Completed ||
            ls.Status == ELogSheetStatus.NeedsReview);
}