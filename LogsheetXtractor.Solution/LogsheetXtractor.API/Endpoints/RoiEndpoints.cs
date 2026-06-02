using FluentResults;
using LogsheetXtractor.API.Extensions;
using LogsheetXtractor.Application.Features.ROIs;
using LogsheetXtractor.Application.Features.ROIs.DTOs;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using Wolverine.Http;

namespace LogsheetXtractor.API.Endpoints;

/// <summary>
/// Request payload for replacing template ROIs.
/// </summary>
/// <param name="Rois">
/// The complete ROI set to store for the template, including coordinates and semantic metadata.
/// </param>
public sealed record SetRoiRequest(IEnumerable<SetRoiDto> Rois);

/// <summary>
/// Endpoints for region-of-interest (ROI) management on templates.
/// </summary>
public static class RoiEndpoints
{
    /// <summary>
    /// Replaces the ROI collection for a template.
    /// ROIs define rectangular regions used during extraction and proofreading.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="request">Payload containing ROIs to persist.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <param name="bus">Message bus used to dispatch the command.</param>
    /// <returns>The stored ROI list when the operation succeeds.</returns>
    [WolverinePost("/api/templates/{templateId}/rois/set")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public static async Task<IResult> SetRoisForTemplate(
        Guid templateId,
        SetRoiRequest request,
        CancellationToken ct,
        IMessageBus bus
    )
    {
        var command = new SetTemplateRoisCommand(templateId, request.Rois);

        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(command, ct);
        return result.ToHttpResult();
    }

    /// <summary>
    /// Lists all ROIs for a template, including their validation configuration.
    /// </summary>
    /// <param name="templateId">The template identifier.</param>
    /// <param name="bus">Message bus used to dispatch the query.</param>
    /// <param name="ct">Cancellation token for the request.</param>
    /// <returns>A collection of template ROIs.</returns>
    [WolverineGet("/api/templates/{templateId}/rois")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public static async Task<IResult> GetRoisForTemplate(
        Guid templateId,
        IMessageBus bus,
        CancellationToken ct
    )
    {
        var query = new ListRoisForTemplateQuery(templateId);

        var result = await bus.InvokeAsync<Result<IEnumerable<RoiDto>>>(query, ct);

        return result.ToHttpResult();
    }
}
