using FluentResults;

namespace LogsheetXtractor.Application.Errors;

public class ValidationError(string message) : Error(message);
