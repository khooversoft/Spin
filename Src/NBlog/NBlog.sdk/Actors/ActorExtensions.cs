using Toolbox.Tools;
using Toolbox.Types;

namespace NBlog.sdk;

public static class ActorExtensions
{
    public static T GetResourceGain<T>(this IClusterClient client, string fileId) where T : IGrainWithStringKey
    {
        client.NotNull();
        fileId.NotEmpty();

        string actorKey = FileId.Create(fileId).ThrowOnError().Return().ToString().ToLower();
        return client.GetGrain<T>(actorKey);
    }

    public static INBlogConfigurationActor GetConfigurationActor(this IClusterClient client) => client.GetResourceGain<INBlogConfigurationActor>(NBlogConstants.ConfigurationActorId);
    public static IArticleManifestActor GetArticleManifestActor(this IClusterClient client, string articleId) => client.GetResourceGain<IArticleManifestActor>(articleId);
    public static IStorageActor GetStorageActor(this IClusterClient client, string fileId) => client.GetResourceGain<IStorageActor>(fileId);
}
