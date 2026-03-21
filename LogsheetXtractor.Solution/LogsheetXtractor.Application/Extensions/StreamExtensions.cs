namespace LogsheetXtractor.Application.Extensions;

public static class StreamExtensions
{
    public static byte[] ToByteArray(this Stream input)
    {
        if (input is MemoryStream ms)
        {
            return ms.ToArray();
        }

        using var memoryStream = new MemoryStream();
        input.CopyTo(memoryStream);
        return memoryStream.ToArray();
    }
}