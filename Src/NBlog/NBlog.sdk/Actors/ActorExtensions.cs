using Toolbox.Tools;

namespace NBlog.sdk;

public static class ActorExtensions
{
    public static INBlogConfigurationActor GetConfigurationActor(this IClusterClient client) => client.NotNull().GetGrain<INBlogConfigurationActor>(NBlogConstants.ConfigurationActorId);
    public static IArticleManifestActor GetArticleManifestActor(this IClusterClient client, string articleId) => client.NotNull().GetGrain<IArticleManifestActor>(articleId.NotEmpty());
}
