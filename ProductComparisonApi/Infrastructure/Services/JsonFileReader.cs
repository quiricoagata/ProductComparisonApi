using ProductComparisonApi.Domain.Interfaces;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ProductComparisonApi.Infrastructure.Services
{
    public class JsonFileReader : IJsonFileReader
    {
        public string ReadAllText(string path) => File.ReadAllText(path);
        public bool FileExists(string path) => File.Exists(path);

        public async Task WriteAllTextAsync(string path, string content)
        {
            ValidateWriteParameters(path, content);
            await File.WriteAllTextAsync(path, content);
        }

        private void ValidateWriteParameters(string path, string content)
        {
            if (path is null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("El path no puede estar vacío.", nameof(path));
            if (content is null) throw new ArgumentNullException(nameof(content));
        }
    }
}