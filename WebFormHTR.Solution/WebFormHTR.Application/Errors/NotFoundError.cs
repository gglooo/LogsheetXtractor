using FluentResults;

namespace WebFormHTR.Application.Errors;

public class NotFoundError(string message) : Error(message);
