using FluentResults;
using WebFormHTR.Application.Features.RoiValidation.DTOs;
using WebFormHTR.Domain.Enums;
using WebFormHTR.Domain.ValueObjects.RoiValidation;

namespace WebFormHTR.Application.Features.RoiValidation;

public sealed class RoiValidationConditionEvaluator(IEnumerable<IRoiValidationConditionNodeEvaluator> nodeEvaluators)
    : IRoiValidationConditionEvaluator
{
    private readonly IReadOnlyList<IRoiValidationConditionNodeEvaluator> _nodeEvaluators = nodeEvaluators.ToList();

    public IReadOnlyList<RoiValidationWarningDto> Evaluate(
        ERoiType roiType,
        string? value,
        RoiValidationConditionNode? conditionRoot)
    {
        if (conditionRoot is null)
        {
            return [];
        }

        var context = new RoiValidationRuleEvaluationContext(roiType, value);
        var result = EvaluateNode(conditionRoot, context, "root");
        return result.ToWarnings();
    }

    private Result EvaluateNode(
        RoiValidationConditionNode node,
        RoiValidationRuleEvaluationContext context,
        string path)
    {
        var evaluator = _nodeEvaluators.FirstOrDefault(x => x.CanHandle(node));
        if (evaluator is null)
        {
            return Failure(path, "validation.node.invalid", "Invalid validation node.");
        }

        return evaluator.Evaluate(node, context, path, (child, childPath) => EvaluateNode(child, context, childPath));
    }

    private static Result Failure(string path, string code, string message)
    {
        return Result.Fail(new RoiValidationWarningError(code, message, path));
    }
}
