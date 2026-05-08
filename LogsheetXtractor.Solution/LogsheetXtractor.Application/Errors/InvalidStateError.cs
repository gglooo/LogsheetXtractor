using FluentResults;

namespace LogsheetXtractor.Application.Errors;

public class InvalidStateError(string message) : Error(message);