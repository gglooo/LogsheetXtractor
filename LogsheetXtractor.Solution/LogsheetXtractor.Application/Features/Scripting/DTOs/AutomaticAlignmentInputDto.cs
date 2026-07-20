using LogsheetXtractor.Application.Features.Logsheets.DTOs;
using LogsheetXtractor.Domain.Entities;

namespace LogsheetXtractor.Application.Features.Scripting.DTOs;

/// <summary>
/// Input passed to the automatic-alignment script for a logsheet.
/// <param name="Logsheet">The logsheet to align with its template.</param>
/// </summary>
public record AutomaticAlignmentInputDto(Logsheet Logsheet);
