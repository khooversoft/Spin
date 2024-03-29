﻿using Toolbox.Tools;
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

    public static IArticleManifestActor GetArticleManifestActor(this IClusterClient client, string articleId) => client.GetResourceGain<IArticleManifestActor>(articleId);
    public static IStorageActor GetStorageActor(this IClusterClient client, string fileId) => client.GetResourceGain<IStorageActor>(fileId);
    public static IDirectoryActor GetDirectoryActor(this IClusterClient client) => client.GetResourceGain<IDirectoryActor>(NBlogConstants.DirectoryActorKey);
    public static IContactRequestActor GetContactRequestActor(this IClusterClient client) => client.GetGrain<IContactRequestActor>(0);

    public static IConfigurationActor GetConfigurationActor(this IClusterClient client, string dbName)
    {
        return client.GetResourceGain<IConfigurationActor>(NBlogConstants.Tool.CreateConfigurationActorKey(dbName));
    }

    public static ISearchActor GetSearchActor(this IClusterClient client, string dbName)
    {
        return client.GetResourceGain<ISearchActor>(NBlogConstants.Tool.CreateSearchIndexActorKey(dbName));
    }

    public static IProfanityFilterActor GetProfanityFilterActor(this IClusterClient client) => client.GetResourceGain<IProfanityFilterActor>(NBlogConstants.BadWordsActorKey);
}
