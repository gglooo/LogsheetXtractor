using System.Runtime.InteropServices;
using Docnet.Core;
using Docnet.Core.Models;
using SkiaSharp;
using WebFormHTR.Application.Interfaces;
using ZXing;
using ZXing.Common;
using ZXing.SkiaSharp;

namespace WebFormHTR.Infrastructure.Services;

public class PdfQrCodeScanner : IPdfQrCodeScanner
{
    private readonly BarcodeReader _reader = new()
    {
        AutoRotate = true,
        Options = new DecodingOptions
        {
            TryHarder = true,
            PossibleFormats = new List<BarcodeFormat> { BarcodeFormat.QR_CODE }
        }
    };

    public Dictionary<int, string> DetectTemplates(byte[] fileBytes)
    {
        var templates = new Dictionary<int, string>();
        using var docReader = DocLib.Instance.GetDocReader(fileBytes, new PageDimensions(2));

        for (var i = 0; i < docReader.GetPageCount(); i++)
        {
            using var pageReader = docReader.GetPageReader(i);

            var rawBytes = pageReader.GetImage();
            var width = pageReader.GetPageWidth();
            var height = pageReader.GetPageHeight();

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

            var handle = GCHandle.Alloc(rawBytes, GCHandleType.Pinned);
            try
            {
                using var bitmap = new SKBitmap();
                bitmap.InstallPixels(info, handle.AddrOfPinnedObject(), info.RowBytes);

                var result = _reader.Decode(bitmap);

                if (result != null)
                {
                    templates.Add(i + 1, result.Text);
                }
            }
            finally
            {
                handle.Free();
            }
        }

        return templates;
    }
}