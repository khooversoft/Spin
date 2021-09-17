using Toolbox.Tools;

namespace Spin.Common.Configuration
{
    public static class QueueModelExtensions
    {
        public static void Verify(this QueueModel queueModel)
        {
            queueModel.VerifyNotNull(nameof(queueModel));

            queueModel.Namespace.VerifyNotEmpty($"{nameof(queueModel.Namespace)} is required");
            queueModel.Name.VerifyNotEmpty($"{nameof(queueModel.Name)} is required");
        }
    }
}