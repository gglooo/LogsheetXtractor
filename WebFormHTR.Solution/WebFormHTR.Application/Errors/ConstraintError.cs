using FluentResults;

namespace WebFormHTR.Application.Errors;

public class ConstraintError(string message) : Error(message);