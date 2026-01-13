using System.Runtime.InteropServices;
using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using WebFormHTR.Application.Features.PdfImage;

namespace WebFormHTR.Infrastructure.Services;

public class PdfCropperService(IDocLib docLib) : IPdfCropperService
{
    public Stream GetCroppedSection(byte[] bytes, int pageNumber, int x, int y, int width, int height,
        int refTotalWidth,
        int refTotalHeight,
        CancellationToken ct)
    {
        using var docReader = docLib.GetDocReader(bytes, new PageDimensions(2.0));
        using var pageReader = docReader.GetPageReader(pageNumber);

        var rawBytes = pageReader.GetImage();
        var actualWidth = pageReader.GetPageWidth();
        var actualHeight = pageReader.GetPageHeight();

        ct.ThrowIfCancellationRequested();

        var scaleX = (double)actualWidth / refTotalWidth;
        var scaleY = (double)actualHeight / refTotalHeight;

        var finalX = (int)(x * scaleX);
        var finalY = (int)(y * scaleY);
        var finalWidth = (int)(width * scaleX);
        var finalHeight = (int)(height * scaleY);

        var info = new SKImageInfo(actualWidth, actualHeight, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var originalBitmap = new SKBitmap(info);

        var pixelsAddr = originalBitmap.GetPixels();
        Marshal.Copy(rawBytes, 0, pixelsAddr, rawBytes.Length);

        var cropRect = new SKRectI(finalX, finalY, finalX + finalWidth, finalY + finalHeight);
        var imageRect = new SKRectI(0, 0, actualWidth, actualHeight);

        cropRect.Intersect(imageRect);
        if (cropRect.IsEmpty)
        {
            throw new InvalidOperationException("Crop rectangle is empty or outside image bounds.");
        }

        using var croppedBitmap = new SKBitmap(cropRect.Width, cropRect.Height);
        if (!originalBitmap.ExtractSubset(croppedBitmap, cropRect))
        {
            throw new InvalidOperationException("Failed to extract image subset.");
        }

        var outputStream = new MemoryStream();
        using var data = croppedBitmap.Encode(SKEncodedImageFormat.Png, 100);
        data.SaveTo(outputStream);
        outputStream.Position = 0;

        return outputStream;
    }
}