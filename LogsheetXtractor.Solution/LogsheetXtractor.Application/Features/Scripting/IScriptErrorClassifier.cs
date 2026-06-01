namespace LogsheetXtractor.Application.Features.Scripting;

public interface IScriptErrorClassifier
{
    ScriptFailureKind ClassifyProcessLogsheetFailure(string rawError);
}
