using Directory.sdk.Service;
using Toolbox.Tools;

namespace Directory.sdk.Model
{
    public record QueueRecord
    {
        public string Channel { get; init; } = null!;

        public bool AutoComplete { get; init; }

        public int MaxConcurrentCalls { get; init; } = 10;

        public string Namespace { get; init; } = null!;

        public string QueueName { get; init; } = null!;

        public string AuthSendListen { get; init; } = null!;

        public string? AuthManage { get; init; }
    }


    public static class QueueRecordExtensions
    {
        public static QueueRecord Verify(this QueueRecord subject)
        {
            subject.VerifyNotNull(nameof(subject));

            subject.Channel.IsDirectoryIdValid().VerifyAssert(x => x.Valid, x => x.Message);
            subject.Namespace.VerifyNotEmpty($"{nameof(subject.Namespace)} is required");
            subject.QueueName.VerifyNotEmpty($"{nameof(subject.QueueName)} is required");
            subject.AuthSendListen.VerifyNotEmpty($"{nameof(subject.AuthSendListen)} is required");

            return subject;
        }
    }
}