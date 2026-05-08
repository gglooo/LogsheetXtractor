using FluentResults;
using LogsheetXtractor.Application.Features.Scripting.DTOs;
using LogsheetXtractor.Domain.ValueObjects;

namespace LogsheetXtractor.Application.Features.PdfCropper;

public interface IPdfCropperService
{
    Result<Stream> GetCroppedSection(
        byte[] bytes,
        int pageNumber,
        int x,
        int y,
        int width,
        int height,
        int refTotalWidth,
        int refTotalHeight,
        CancellationToken ct
    );

    int GetPageCount(byte[] bytes, CancellationToken ct);

    Result<Stream> GetWarpedSection(
        byte[] bytes,
        int pageNumber,
        IEnumerable<PointCoordinate> srcPoints,
        IEnumerable<PointCoordinate> dstPoints,
        int width,
        int height,
        int referenceWidth,
        int referenceHeight,
        CancellationToken ct
    );
}
