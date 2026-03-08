using Microsoft.Extensions.Configuration;
using ProductComparisonApi.Domain.Interfaces;

namespace ProductComparisonApi.Infrastructure.Services
{
    /// <summary>
    /// Servicio para la lectura y escritura de archivos JSON.
    /// Implementa la interfaz <see cref="IJsonFileReader"/> proporcionando métodos para acceder
    /// al sistema de archivos con validaciones de parámetros de entrada.
    /// </summary>
    public class JsonFileReader : IJsonFileReader
    {
        public string JsonPath { get; }

        public JsonFileReader(IConfiguration configuration)
        {
            var dataPath = configuration["DATA_PATH"] ?? AppContext.BaseDirectory;
            JsonPath = Path.Combine(dataPath, "Data", "products.json");
        }

        public string ReadAllText(string path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));
            
            if (!File.Exists(path))
                throw new FileNotFoundException("No se encontró el archivo.", path);
            
            return File.ReadAllText(path);
        }

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