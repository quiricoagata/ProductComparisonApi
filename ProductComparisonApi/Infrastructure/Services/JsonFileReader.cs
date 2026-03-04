using ProductComparisonApi.Domain.Interfaces;

namespace ProductComparisonApi.Infrastructure.Services
{

    public class JsonFileReader : IJsonFileReader
    {
        public string ReadAllText(string path) => File.ReadAllText(path);
        public bool FileExists(string path) => File.Exists(path);

        public async Task WriteAllTextAsync(string path, string content) =>
            await File.WriteAllTextAsync(path, content);
    }
}