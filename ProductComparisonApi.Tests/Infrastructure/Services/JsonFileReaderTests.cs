using ProductComparisonApi.Infrastructure.Services;
using System.Text;

namespace ProductComparisonApi.Tests.Infrastructure.Services
{
    public class JsonFileReaderTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _testFilePath;

        public JsonFileReaderTests()
        {
            // Crear directorio temporal para los tests
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"JsonFileReaderTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
            _testFilePath = Path.Combine(_tempDirectory, "test.json");
        }

        public void Dispose()
        {
            // Limpiar los archivos y directorios temporales después de cada test
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        // ── FileExists ─────────────────────────────────────────────────

        [Fact]
        public void FileExists_ArchivoExiste_RetornaTrue()
        {
            // Arrange
            var reader = new JsonFileReader();
            File.WriteAllText(_testFilePath, "test content");

            // Act
            var result = reader.FileExists(_testFilePath);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void FileExists_ArchivoNoExiste_RetornaFalse()
        {
            // Arrange
            var reader = new JsonFileReader();
            var nonExistentPath = Path.Combine(_tempDirectory, "no_existe.json");

            // Act
            var result = reader.FileExists(nonExistentPath);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FileExists_RutaVacia_RetornaFalse()
        {
            // Arrange
            var reader = new JsonFileReader();

            // Act
            var result = reader.FileExists("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FileExists_DirectorioEnLugarDeArchivo_RetornaFalse()
        {
            // Arrange
            var reader = new JsonFileReader();

            // Act
            var result = reader.FileExists(_tempDirectory);

            // Assert
            Assert.False(result);
        }

        // ── ReadAllText ────────────────────────────────────────────────

        [Fact]
        public void ReadAllText_ArchivoExiste_RetornaContenido()
        {
            // Arrange
            var reader = new JsonFileReader();
            var contenidoEsperado = "{ \"id\": 1, \"nombre\": \"Test\" }";
            File.WriteAllText(_testFilePath, contenidoEsperado);

            // Act
            var result = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(contenidoEsperado, result);
        }

        [Fact]
        public void ReadAllText_ArchivoVacio_RetornaStringVacio()
        {
            // Arrange
            var reader = new JsonFileReader();
            File.WriteAllText(_testFilePath, "");

            // Act
            var result = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal("", result);
        }

        [Fact]
        public void ReadAllText_ArchivoConContenidoMultilinea_RetornaContenidoCompleto()
        {
            // Arrange
            var reader = new JsonFileReader();
            var contenidoEsperado = "{\n  \"id\": 1,\n  \"nombre\": \"Producto\"\n}";
            File.WriteAllText(_testFilePath, contenidoEsperado);

            // Act
            var result = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(contenidoEsperado, result);
        }

        [Fact]
        public void ReadAllText_ArchivoConCaracteresEspeciales_RetornaContenidoCorrectamente()
        {
            // Arrange
            var reader = new JsonFileReader();
            var contenidoEsperado = "{ \"nombre\": \"Laptop Pro X1\", \"descripción\": \"Equipo muy especial!\" }";
            File.WriteAllText(_testFilePath, contenidoEsperado, Encoding.UTF8);

            // Act
            var result = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(contenidoEsperado, result);
        }

        [Fact]
        public void ReadAllText_ArchivoNoExiste_LanzaFileNotFoundException()
        {
            // Arrange
            var reader = new JsonFileReader();
            var rutaNoExistente = Path.Combine(_tempDirectory, "no_existe.json");

            // Act & Assert
            Assert.Throws<FileNotFoundException>(() => reader.ReadAllText(rutaNoExistente));
        }

        [Fact]
        public void ReadAllText_RutaVacia_LanzaArgumentException()
        {
            // Arrange
            var reader = new JsonFileReader();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => reader.ReadAllText(""));
        }

        [Fact]
        public void ReadAllText_RutaNull_LanzaArgumentNullException()
        {
            // Arrange
            var reader = new JsonFileReader();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => reader.ReadAllText(null!));
        }

        [Fact]
        public void ReadAllText_ArchivoPequeño_RetornaContenidoRapidamente()
        {
            // Arrange
            var reader = new JsonFileReader();
            var contenido = "{ \"test\": \"pequeño\" }";
            File.WriteAllText(_testFilePath, contenido);

            // Act
            var result = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(contenido, result);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ReadAllText_ArchivoGrande_RetornaContenidoCompleto()
        {
            // Arrange
            var reader = new JsonFileReader();
            var sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < 100; i++)
            {
                if (i > 0) sb.Append(",");
                sb.Append($"{{\"id\":{i},\"nombre\":\"Producto {i}\"}}");
            }
            sb.Append("]");
            var contenidoEsperado = sb.ToString();
            File.WriteAllText(_testFilePath, contenidoEsperado);

            // Act
            var result = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(contenidoEsperado, result);
        }

        [Fact]
        public void ReadAllText_DosSolicitudesConsecutivas_RetornanMismoContenido()
        {
            // Arrange
            var reader = new JsonFileReader();
            var contenido = "{ \"id\": 1 }";
            File.WriteAllText(_testFilePath, contenido);

            // Act
            var result1 = reader.ReadAllText(_testFilePath);
            var result2 = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(contenido, result1);
        }

        // ── WriteAllTextAsync ──────────────────────────────────────────

        [Fact]
        public async Task WriteAllTextAsync_EscribeContenido_CreaArchivo()
        {
            // Arrange
            var reader = new JsonFileReader();
            var contenido = "contenido async";

            // Act
            await reader.WriteAllTextAsync(_testFilePath, contenido);

            // Assert
            var read = File.ReadAllText(_testFilePath);
            Assert.Equal(contenido, read);
        }

        [Fact]
        public async Task WriteAllTextAsync_SobrescribeContenido()
        {
            // Arrange
            File.WriteAllText(_testFilePath, "antiguo");
            var reader = new JsonFileReader();

            // Act
            await reader.WriteAllTextAsync(_testFilePath, "nuevo");

            // Assert
            var read = File.ReadAllText(_testFilePath);
            Assert.Equal("nuevo", read);
        }

        [Fact]
        public async Task WriteAllTextAsync_ContenidoNull_LanzaArgumentNullException()
        {
            // Arrange
            var reader = new JsonFileReader();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => reader.WriteAllTextAsync(_testFilePath, null!));
        }

        [Fact]
        public async Task WriteAllTextAsync_RutaVacia_LanzaArgumentException()
        {
            // Arrange
            var reader = new JsonFileReader();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => reader.WriteAllTextAsync("", "algo"));
        }
    }
}