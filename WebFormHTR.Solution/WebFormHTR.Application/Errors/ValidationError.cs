using FluentResults;

namespace WebFormHTR.Application.Errors;

public class ValidationError(string message) : Error(message);
