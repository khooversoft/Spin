namespace Toolbox.Azure.DataLake
{
    public interface IDataLakeStoreFactory
    {
        IDataLakeStore? Create(string nameSpace);
    }
}