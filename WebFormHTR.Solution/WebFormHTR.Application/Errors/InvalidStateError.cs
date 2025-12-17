using FluentResults;

namespace WebFormHTR.Application.Errors;

public class InvalidStateError(string message) : Error(message);