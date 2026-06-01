using FluentResults;

namespace LogsheetXtractor.Application.Errors;

public class NotFoundError(string message) : Error(message);