namespace ProductComparisonApi.Domain.Interfaces
{

    public interface IJsonFileReader
    {
        string ReadAllText(string path);

        bool FileExists(string path);

        Task WriteAllTextAsync(string path, string content);
    }
}