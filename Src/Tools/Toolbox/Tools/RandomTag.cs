namespace Toolbox.Tools;

public class RandomTag
{
    private Random _random = new Random();
    private object _object = new object();

    public string Get(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        lock (_object)
        {
            var data = Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)])
                .ToArray();

            return new string(data);
        }
    }
}
