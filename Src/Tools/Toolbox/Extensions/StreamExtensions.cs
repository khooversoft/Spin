namespace Toolbox.Extensions;

public static class StreamExtensions
{
    public static async Task<string> ReadStringStreamAsync(this Stream stream)
    {
        using StreamReader sr = new StreamReader(stream);
        return await sr.ReadToEndAsync();
    }

    public static string ReadStringStream(this Stream stream)
    {
        using StreamReader sr = new StreamReader(stream);
        return sr.ReadToEnd();
    }
}
