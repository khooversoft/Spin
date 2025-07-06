using Toolbox.Data;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace Toolbox.Test.Data.Journal;

public class JournalEntryTest
{
    [Fact]
    public void SerializationTest1()
    {
        var subject = new JournalEntry();

        var json = subject.ToJson();
        var s2 = json.ToObject<JournalEntry>();
        s2.NotNull();
        (subject == s2).BeTrue();
    }

    [Fact]
    public void SerializationTest2()
    {
        var json = "{\"logSequenceNumber\":\"20250107-08-0000000003-ABB9\",\"transactionId\":\"7562623c-216f-46f3-9806-825390d822bd\",\"timeStamp\":\"2025-01-07T08:12:28.2797409Z\",\"type\":\"commit\",\"data\":{}}";
        var s2 = json.ToObject<JournalEntry>();
        s2.NotNull();
    }
}
