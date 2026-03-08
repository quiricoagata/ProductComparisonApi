using Microsoft.Extensions.Configuration;
using Moq;
using ProductComparisonApi.Infrastructure.Repositories;
using System.Text;

namespace ProductComparisonApi.Tests.Infrastructure.Repositories
{
    public class JsonFileReaderTests : IDisposable
    {
        private readonly string _tempDirectory;
        private readonly string _testFilePath;
        private readonly Mock<IConfiguration> _mockConfiguration;

        public JsonFileReaderTests()
        {
            // Crear directorio temporal para los tests
            _tempDirectory = Path.Combine(Path.GetTempPath(), $"JsonFileReaderTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
            _testFilePath = Path.Combine(_tempDirectory, "test.json");
            
            // Mock de configuración
            _mockConfiguration = new Mock<IConfiguration>();
            _mockConfiguration.Setup(c => c["DATA_PATH"]).Returns(AppContext.BaseDirectory);
        }

        public void Dispose()
        {
            // Limpiar los archivos y directorios temporales después de cada test
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, recursive: true);
            }
        }

        // ?? Constructor ????????????????????????????????????????????????

        [Fact]
        public void Constructor_ConConfiguration_InicializaJsonPath()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["DATA_PATH"]).Returns(AppContext.BaseDirectory);

            // Act
            var reader = new JsonFileReader(config.Object);

            // Assert
            Assert.NotNull(reader.JsonPath);
            Assert.Contains("Data", reader.JsonPath);
            Assert.EndsWith("products.json", reader.JsonPath);
        }

        [Fact]
        public void Constructor_DataPathNull_UsaAppContextBaseDirectory()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["DATA_PATH"]).Returns((string?)null);

            // Act
            var reader = new JsonFileReader(config.Object);

            // Assert
            Assert.NotNull(reader.JsonPath);
            Assert.StartsWith(AppContext.BaseDirectory, reader.JsonPath);
        }

        [Fact]
        public void Constructor_DataPathEspecificado_UsaDataPath()
        {
            // Arrange
            var config = new Mock<IConfiguration>();
            config.Setup(c => c["DATA_PATH"]).Returns("/custom/path");

            // Act
            var reader = new JsonFileReader(config.Object);

            // Assert
            Assert.StartsWith("/custom/path", reader.JsonPath);
        }

        // ?? FileExists ?????????????????????????????????????????????????

        [Fact]
        public void FileExists_ArchivoExiste_RetornaTrue()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act
            var result = reader.FileExists("");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FileExists_RutaNull_RetornaFalse()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act
            var result = reader.FileExists(null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void FileExists_DirectorioEnLugarDeArchivo_RetornaFalse()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act
            var result = reader.FileExists(_tempDirectory);

            // Assert
            Assert.False(result);
        }

        // ?? ReadAllText ????????????????????????????????????????????????

        [Fact]
        public void ReadAllText_ArchivoExiste_RetornaContenido()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
            var rutaNoExistente = Path.Combine(_tempDirectory, "no_existe.json");

            // Act & Assert
            var ex = Assert.Throws<FileNotFoundException>(() => reader.ReadAllText(rutaNoExistente));
            Assert.Equal("No se encontró el archivo.", ex.Message);
        }

        [Fact]
        public void ReadAllText_RutaNull_LanzaArgumentNullException()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => reader.ReadAllText(null!));
        }

        [Fact]
        public void ReadAllText_ArchivoPequeño_RetornaContenidoRapidamente()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);
            var contenido = "{ \"id\": 1 }";
            File.WriteAllText(_testFilePath, contenido);

            // Act
            var result1 = reader.ReadAllText(_testFilePath);
            var result2 = reader.ReadAllText(_testFilePath);

            // Assert
            Assert.Equal(result1, result2);
            Assert.Equal(contenido, result1);
        }

        // ?? WriteAllTextAsync ??????????????????????????????????????????

        [Fact]
        public async Task WriteAllTextAsync_EscribeContenido_CreaArchivo()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);
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
            var reader = new JsonFileReader(_mockConfiguration.Object);

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
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => 
                reader.WriteAllTextAsync(_testFilePath, null!));
            Assert.Equal("content", ex.ParamName);
        }

        [Fact]
        public async Task WriteAllTextAsync_RutaNull_LanzaArgumentNullException()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(() => 
                reader.WriteAllTextAsync(null!, "contenido"));
            Assert.Equal("path", ex.ParamName);
        }

        [Fact]
        public async Task WriteAllTextAsync_RutaVacia_LanzaArgumentException()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
                reader.WriteAllTextAsync("", "algo"));
            Assert.Equal("path", ex.ParamName);
            Assert.Contains("vacío", ex.Message);
        }

        [Fact]
        public async Task WriteAllTextAsync_RutaWhitespace_LanzaArgumentException()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ArgumentException>(() => 
                reader.WriteAllTextAsync("   ", "contenido"));
            Assert.Equal("path", ex.ParamName);
        }

        [Fact]
        public async Task WriteAllTextAsync_ContenidoVacio_NoLanzaExcepcion()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act & Assert - contenido vacío es válido
            await reader.WriteAllTextAsync(_testFilePath, "");
            var content = File.ReadAllText(_testFilePath);
            Assert.Equal("", content);
        }

        [Fact]
        public async Task WriteAllTextAsync_ContenidoConCaracteresEspeciales_EscribeCorrectamente()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);
            var contenido = "{ \"nombre\": \"Laptop Ágata\", \"emoji\": \"??\" }";

            // Act
            await reader.WriteAllTextAsync(_testFilePath, contenido);

            // Assert
            var read = File.ReadAllText(_testFilePath, Encoding.UTF8);
            Assert.Equal(contenido, read);
        }

        [Fact]
        public async Task WriteAllTextAsync_MultiplesLlamadas_SobrescribeCorrectamente()
        {
            // Arrange
            var reader = new JsonFileReader(_mockConfiguration.Object);

            // Act
            await reader.WriteAllTextAsync(_testFilePath, "contenido 1");
            await reader.WriteAllTextAsync(_testFilePath, "contenido 2");
            await reader.WriteAllTextAsync(_testFilePath, "contenido 3");

            // Assert
            var final = File.ReadAllText(_testFilePath);
            Assert.Equal("contenido 3", final);
        }
    }
}
