using FluentResults;
using LogsheetXtractor.Domain.ValueObjects.RoiValidation;

namespace LogsheetXtractor.Application.Features.RoiValidation;

public sealed class RoiValidationGroupNodeEvaluator : IRoiValidationConditionNodeEvaluator
{
    public bool CanHandle(RoiValidationConditionNode node)
    {
        return string.Equals(node.Type, "group", StringComparison.OrdinalIgnoreCase);
    }

    public Result Evaluate(
        RoiValidationConditionNode node,
        RoiValidationRuleEvaluationContext context,
        string path,
        Func<RoiValidationConditionNode, string, Result> evaluateChild
    )
    {
        if (node.Children is null || node.Children.Count == 0)
        {
            return Failure(path, "validation.group.empty", "Condition group has no children.");
        }

        var childResults = new List<Result>(node.Children.Count);
        for (var i = 0; i < node.Children.Count; i++)
        {
            childResults.Add(evaluateChild(node.Children[i], $"{path}.children[{i}]"));
        }

        if (string.Equals(node.Operator, "OR", StringComparison.OrdinalIgnoreCase))
        {
            if (childResults.Any(x => x.IsSuccess))
            {
                return Result.Ok();
            }

            return Result.Fail(childResults.SelectMany(x => x.Errors));
        }

        if (string.Equals(node.Operator, "AND", StringComparison.OrdinalIgnoreCase))
        {
            var andSuccess = childResults.All(x => x.IsSuccess);
            return andSuccess ? Result.Ok() : Result.Fail(childResults.SelectMany(x => x.Errors));
        }

        return Failure(path, "validation.group.operator", "Invalid group operator.");
    }

    private static Result Failure(string path, string code, string message)
    {
        return Result.Fail(new RoiValidationWarningError(code, message, path));
    }
}

public sealed class RoiValidationRuleNodeEvaluator(IRoiValidationRuleRegistry registry)
    : IRoiValidationConditionNodeEvaluator
{
    public bool CanHandle(RoiValidationConditionNode node)
    {
        return string.Equals(node.Type, "rule", StringComparison.OrdinalIgnoreCase);
    }

    public Result Evaluate(
        RoiValidationConditionNode node,
        RoiValidationRuleEvaluationContext context,
        string path,
        Func<RoiValidationConditionNode, string, Result> evaluateChild
    )
    {
        if (string.IsNullOrWhiteSpace(node.RuleType))
        {
            return Failure(path, "validation.rule.missing", "Rule type is required.");
        }

        if (!registry.TryGet(node.RuleType, out var evaluator))
        {
            return Failure(
                path,
                "validation.rule.unsupported",
                $"Rule '{node.RuleType}' is not supported."
            );
        }

        if (!evaluator.SupportedRoiTypes.Contains(context.RoiType))
        {
            return Failure(
                path,
                "validation.rule.incompatible",
                $"Rule '{node.RuleType}' is not supported for ROI type '{context.RoiType}'."
            );
        }

        return evaluator.Evaluate(context, node.Params, path);
    }

    private static Result Failure(string path, string code, string message)
    {
        return Result.Fail(new RoiValidationWarningError(code, message, path));
    }
}
