namespace ArtifactStore.sdk.Services
{
    public interface IArtifactStorageFactory
    {
        IArtifactStorage? Create(string nameSpace);
    }
}