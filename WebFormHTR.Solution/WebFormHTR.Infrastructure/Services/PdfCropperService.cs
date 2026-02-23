using System.Runtime.InteropServices;
using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using WebFormHTR.Application.Features.PdfCropper;
using WebFormHTR.Application.Features.Scripting.DTOs;
using WebFormHTR.Application.Interfaces;
using WebFormHTR.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace WebFormHTR.Infrastructure.Services;

public class PdfCropperService(
    IDocLib docLib,
    IPerspectiveMatrixComputer perspectiveMatrixComputer,
    ILogger<PdfCropperService> logger)
    : IPdfCropperService
{
    private const double RenderScale = 2.0;

    public int GetPageCount(byte[] bytes, CancellationToken ct)
    {
        using var docReader = docLib.GetDocReader(bytes, new PageDimensions(1.0));
        ct.ThrowIfCancellationRequested();
        return docReader.GetPageCount();
    }

    public PdfDimensionsDto GetPageDimensions(byte[] bytes, int pageNumber, CancellationToken ct)
    {
        using var docReader = docLib.GetDocReader(bytes, new PageDimensions(2.0));
        using var pageReader = docReader.GetPageReader(pageNumber);

        ct.ThrowIfCancellationRequested();

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        return new PdfDimensionsDto
        {
            Width = width,
            Height = height
        };
    }

    public Stream GetCroppedSection(byte[] bytes, int pageNumber, int x, int y, int width, int height,
        int refTotalWidth,
        int refTotalHeight,
        CancellationToken ct)
    {
        using var docReader = docLib.GetDocReader(bytes, new PageDimensions(RenderScale));
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

        using var ctx = new BitmapContext(rawBytes, actualWidth, actualHeight);

        var cropRect = new SKRectI(finalX, finalY, finalX + finalWidth, finalY + finalHeight);
        var imageRect = new SKRectI(0, 0, actualWidth, actualHeight);

        cropRect.Intersect(imageRect);
        if (cropRect.IsEmpty)
        {
            logger.LogError("Crop rectangle is empty or outside image bounds. FinalX: {X}, FinalY: {Y}, Width: {Width}, Height: {Height}", finalX, finalY, finalWidth, finalHeight);
            throw new InvalidOperationException("Crop rectangle is empty or outside image bounds.");
        }

        using var croppedBitmap = new SKBitmap(cropRect.Width, cropRect.Height);
        if (!ctx.Bitmap.ExtractSubset(croppedBitmap, cropRect))
        {
            logger.LogError("Failed to extract image subset from bitmap.");
            throw new InvalidOperationException("Failed to extract image subset.");
        }

        var outputStream = new MemoryStream();
        using var data = croppedBitmap.Encode(SKEncodedImageFormat.Jpeg, 85);
        data.SaveTo(outputStream);
        outputStream.Position = 0;

        return outputStream;
    }

    public Stream GetWarpedSection(byte[] bytes, int pageNumber, IEnumerable<PointCoordinate> srcPoints,
        IEnumerable<PointCoordinate> dstPoints, int width,
        int height, int referenceWidth, int referenceHeight, CancellationToken ct)
    {
        using var docReader = docLib.GetDocReader(bytes, new PageDimensions(2.0));
        using var pageReader = docReader.GetPageReader(pageNumber);

        var rawBytes = pageReader.GetImage();
        var actualWidth = pageReader.GetPageWidth();
        var actualHeight = pageReader.GetPageHeight();

        ct.ThrowIfCancellationRequested();

        using var ctx = new BitmapContext(rawBytes, actualWidth, actualHeight);

        var scaleX = (float)actualWidth / referenceWidth;
        var scaleY = (float)actualHeight / referenceHeight;

        var src = srcPoints.Select(p => new SKPoint(p.X * scaleX, p.Y * scaleY)).ToArray();
        var dst = dstPoints.Select(p => new SKPoint(p.X, p.Y)).ToArray();

        var matrix = perspectiveMatrixComputer.ComputePerspectiveMatrix(src, dst);

        var outputInfo = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
        using var warpedBitmap = new SKBitmap(outputInfo);
        using var canvas = new SKCanvas(warpedBitmap);

        canvas.Clear(SKColors.White);
        canvas.SetMatrix(matrix);

        canvas.DrawBitmap(ctx.Bitmap, 0, 0, new SKPaint
        {
            IsAntialias = true,
            FilterQuality = SKFilterQuality.Medium
        });

        var outputStream = new MemoryStream();
        using var data = warpedBitmap.Encode(SKEncodedImageFormat.Jpeg, 85);
        data.SaveTo(outputStream);
        outputStream.Position = 0;

        return outputStream;
    }

    private class BitmapContext : IDisposable
    {
        private GCHandle _handle;
        public SKBitmap Bitmap { get; }

        public BitmapContext(byte[] data, int width, int height)
        {
            _handle = GCHandle.Alloc(data, GCHandleType.Pinned);
            var ptr = _handle.AddrOfPinnedObject();

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            Bitmap = new SKBitmap();

            Bitmap.InstallPixels(info, ptr, info.RowBytes, null, null);
        }

        public void Dispose()
        {
            Bitmap.Dispose();
            if (_handle.IsAllocated)
            {
                _handle.Free();
            }
        }
    }
}