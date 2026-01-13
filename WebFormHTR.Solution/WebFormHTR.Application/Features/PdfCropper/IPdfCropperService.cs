namespace WebFormHTR.Application.Features.PdfImage;

public interface IPdfCropperService
{
    Stream GetCroppedSection(byte[] bytes, int pageNumber, int x, int y, int width, int height, int refTotalWidth,
        int refTotalHeight,
        CancellationToken ct);
}