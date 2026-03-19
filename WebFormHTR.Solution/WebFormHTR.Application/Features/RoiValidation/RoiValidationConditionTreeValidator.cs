using FluentResults;
using WebFormHTR.Application.Errors;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects.RoiValidation;

namespace WebFormHTR.Application.Features.RoiValidation;

public sealed class RoiValidationConditionTreeValidator(IRoiValidationRuleRegistry registry)
    : IRoiValidationConditionTreeValidator
{
    public Result Validate(ERoiType roiType, RoiValidationConditionNode conditionRoot)
    {
        if (!string.Equals(conditionRoot.Type, "group", StringComparison.OrdinalIgnoreCase))
        {
            return Result.Fail(new ValidationError("Validation condition root must be a group node."));
        }

        return ValidateNode(conditionRoot, roiType, "root", isRoot: true);
    }

    private Result ValidateNode(RoiValidationConditionNode node, ERoiType roiType, string path, bool isRoot)
    {
        if (string.Equals(node.Type, "group", StringComparison.OrdinalIgnoreCase))
        {
            if (!string.Equals(node.Operator, "AND", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(node.Operator, "OR", StringComparison.OrdinalIgnoreCase))
            {
                return Result.Fail(new ValidationError($"Invalid group operator at '{path}'."));
            }

            if (node.Children is null || node.Children.Count == 0)
            {
                return Result.Fail(new ValidationError($"Group at '{path}' must have at least one child."));
            }

            if (!isRoot && node.SchemaVersion is not null)
            {
                return Result.Fail(new ValidationError($"Schema version can be set only at root. Path '{path}'."));
            }

            if (isRoot && node.SchemaVersion is not null && node.SchemaVersion <= 0)
            {
                return Result.Fail(new ValidationError("Schema version must be positive."));
            }

            for (var i = 0; i < node.Children.Count; i++)
            {
                var childResult = ValidateNode(node.Children[i], roiType, $"{path}.children[{i}]", isRoot: false);
                if (childResult.IsFailed)
                {
                    return childResult;
                }
            }

            return Result.Ok();
        }

        if (string.Equals(node.Type, "rule", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(node.RuleType))
            {
                return Result.Fail(new ValidationError($"Rule at '{path}' must define 'ruleType'."));
            }

            if (node.Children is not null)
            {
                return Result.Fail(new ValidationError($"Rule at '{path}' must not define children."));
            }

            if (!registry.TryGet(node.RuleType, out var evaluator))
            {
                return Result.Fail(new ValidationError($"Unsupported rule '{node.RuleType}' at '{path}'."));
            }

            if (!evaluator.SupportedRoiTypes.Contains(roiType))
            {
                return Result.Fail(new ValidationError(
                    $"Rule '{node.RuleType}' is not supported for ROI type '{roiType}' at '{path}'."));
            }

            var paramsValidation = evaluator.ValidateParams(node.Params);
            if (paramsValidation.IsFailed)
            {
                var message = paramsValidation.Errors.FirstOrDefault()?.Message ?? "Invalid rule parameters.";
                return Result.Fail(new ValidationError($"{message} Path '{path}'."));
            }

            return Result.Ok();
        }

        return Result.Fail(new ValidationError($"Unsupported node type '{node.Type}' at '{path}'."));
    }
}
