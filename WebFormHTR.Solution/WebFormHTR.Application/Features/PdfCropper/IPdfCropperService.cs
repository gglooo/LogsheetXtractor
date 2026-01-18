using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Domain.ValueObjects;

namespace WebFormHTR.Application.Features.PdfCropper;

public interface IPdfCropperService
{
    PdfDimensionsDto GetPageDimensions(byte[] bytes, int pageNumber, CancellationToken ct);

    Stream GetCroppedSection(byte[] bytes, int pageNumber, int x, int y, int width, int height, int refTotalWidth,
        int refTotalHeight, CancellationToken ct);

    Stream GetWarpedSection(byte[] bytes, int pageNumber, IEnumerable<PointCoordinate> srcPoints, IEnumerable<PointCoordinate> dstPoints, int width,
        int height, int referenceWidth, int referenceHeight, CancellationToken ct);
}