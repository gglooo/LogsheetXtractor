namespace LogsheetXtractor.Application.Interfaces;

public interface IPdfQrCodeScanner
{
    Dictionary<int, string> DetectTemplates(byte[] fileBytes);
}
