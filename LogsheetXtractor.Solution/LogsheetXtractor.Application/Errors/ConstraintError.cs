using FluentResults;

namespace LogsheetXtractor.Application.Errors;

public class ConstraintError(string message) : Error(message);