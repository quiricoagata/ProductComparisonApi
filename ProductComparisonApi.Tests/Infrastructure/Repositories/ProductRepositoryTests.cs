using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProductComparisonApi.Domain.Models;
using ProductComparisonApi.Infrastructure.Repositories;
using System.Text.Json;

namespace ProductComparisonApi.Tests.Infrastructure.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly Mock<ILogger<ProductRepository>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnv;
        private readonly List<Product> _fakeProducts;
        private readonly string _tempDirectory;
        private readonly string _testJsonPath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ProductRepositoryTests()
        {
            _mockLogger = new();
            _mockEnv = new();

            _fakeProducts = new()
            {
                new() { Id = 1, Nombre = "Laptop Pro X1", Precio = 1299.99m, Calificacion = 4.7, Descripcion = "Desc 1", UrlImagen = "https://img1.com", Especificaciones = new() { { "RAM", "16GB" } } },
                new() { Id = 2, Nombre = "Laptop UltraSlim", Precio = 999.99m, Calificacion = 4.3, Descripcion = "Desc 2", UrlImagen = "https://img2.com", Especificaciones = new() { { "RAM", "8GB" } } },
                new() { Id = 3, Nombre = "Laptop Gaming", Precio = 1899.99m, Calificacion = 4.9, Descripcion = "Desc 3", UrlImagen = "https://img3.com", Especificaciones = new() { { "RAM", "32GB" } } }
            };

            _tempDirectory = Path.Combine(Path.GetTempPath(), $"ProductRepoTests_{Guid.NewGuid()}");
            Directory.CreateDirectory(_tempDirectory);
            
            // Crear la carpeta Data
            var dataFolder = Path.Combine(_tempDirectory, "Data");
            Directory.CreateDirectory(dataFolder);
            _testJsonPath = Path.Combine(dataFolder, "products.json");
            
            // Crear archivo JSON de prueba
            var json = JsonSerializer.Serialize(_fakeProducts, _jsonOptions);
            File.WriteAllText(_testJsonPath, json);
        }

        private ProductRepository CreateRepository()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["DATA_PATH"]).Returns(_tempDirectory);
            
            var fileReader = new JsonFileReader(mockConfig.Object);
            return new ProductRepository(_mockLogger.Object, _mockEnv.Object, fileReader);
        }

        // ?? GetAllAsync ???????????????????????????????????????????????

        [Fact]
        public async Task GetAllAsync_ExistenProductos_RetornaTodosLosProductos()
        {
            var repository = CreateRepository();
            var result = await repository.GetAllAsync();
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ExistenProductos_RetornaListaOrdenadaPorId()
        {
            var repository = CreateRepository();
            var result = await repository.GetAllAsync();
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
            Assert.Equal(3, result[2].Id);
        }

        // ?? GetByIdAsync ??????????????????????????????????????????????

        [Fact]
        public async Task GetByIdAsync_IdExistente_RetornaProductoCorrecto()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdAsync(1);
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Laptop Pro X1", result.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_IdNoExistente_RetornaNull()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdAsync(99);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_IdCero_RetornaNull()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdAsync(0);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_IdNegativo_RetornaNull()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdAsync(-1);
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_TodosLosIds_RetornanProductosCorrectos()
        {
            var repository = CreateRepository();
            
            var product1 = await repository.GetByIdAsync(1);
            var product2 = await repository.GetByIdAsync(2);
            var product3 = await repository.GetByIdAsync(3);
            
            Assert.NotNull(product1);
            Assert.NotNull(product2);
            Assert.NotNull(product3);
            
            Assert.Equal("Laptop Pro X1", product1!.Nombre);
            Assert.Equal("Laptop UltraSlim", product2!.Nombre);
            Assert.Equal("Laptop Gaming", product3!.Nombre);
        }

        // ?? GetByIdsAsync ?????????????????????????????????????????????

        [Fact]
        public async Task GetByIdsAsync_IdsExistentes_RetornaProductosCorrectos()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdsAsync(new List<int> { 1, 3 });
            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == 1);
            Assert.Contains(result, p => p.Id == 3);
        }

        [Fact]
        public async Task GetByIdsAsync_AlgunIdNoExistente_RetornaSoloLosQueExisten()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdsAsync(new List<int> { 1, 99 });
            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetByIdsAsync_NingunIdExistente_RetornaListaVacia()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdsAsync(new List<int> { 98, 99 });
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByIdsAsync_ListaVacia_RetornaListaVacia()
        {
            var repository = CreateRepository();
            var result = await repository.GetByIdsAsync(new List<int>());
            Assert.Empty(result);
        }

        // ?? CreateAsync ???????????????????????????????????????????????

        [Fact]
        public async Task CreateAsync_ProductoValido_RetornaProductoConIdGenerado()
        {
            var repository = CreateRepository();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            var result = await repository.CreateAsync(newProduct);

            Assert.Equal(4, result.Id);
            Assert.Equal("Laptop Nueva", result.Nombre);
        }

        [Fact]
        public async Task CreateAsync_MultipleProductos_GeneraIdsSecuenciales()
        {
            var repository = CreateRepository();
            
            var product1 = new Product { Nombre = "Laptop Nueva 1", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };
            var product2 = new Product { Nombre = "Laptop Nueva 2", Precio = 899m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            var result1 = await repository.CreateAsync(product1);
            var result2 = await repository.CreateAsync(product2);

            Assert.Equal(4, result1.Id);
            Assert.Equal(5, result2.Id);
        }

        [Fact]
        public async Task CreateAsync_ProductoAgregado_SeGuardaEnArchivo()
        {
            var repository = CreateRepository();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await repository.CreateAsync(newProduct);
            
            // Verificar que el producto se agregó
            var allProducts = await repository.GetAllAsync();
            Assert.Equal(4, allProducts.Count);
            Assert.Contains(allProducts, p => p.Nombre == "Laptop Nueva");
        }

        [Fact]
        public async Task CreateAsync_ErrorPersistencia_LanzaExcepcion()
        {
            var repository = CreateRepository();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            // Borrar el directorio para simular error de I/O
            Directory.Delete(_tempDirectory, true);
            
            // CreateAsync debe lanzar excepción porque no puede escribir
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => repository.CreateAsync(newProduct));
        }

        // ?? UpdateAsync ???????????????????????????????????????????????

        [Fact]
        public async Task UpdateAsync_IdExistente_RetornaProductoActualizado()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            var result = await repository.UpdateAsync(1, updated);

            Assert.Equal(1, result.Id);
            Assert.Equal("Laptop Actualizada", result.Nombre);
            Assert.Equal(1099m, result.Precio);
        }

        [Fact]
        public async Task UpdateAsync_IdExistente_ActualizaElDiccionario()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            await repository.UpdateAsync(1, updated);

            var fromDictionary = await repository.GetByIdAsync(1);
            Assert.Equal("Laptop Actualizada", fromDictionary!.Nombre);
            Assert.Equal(1099m, fromDictionary.Precio);
        }

        [Fact]
        public async Task UpdateAsync_IdNoExistente_LanzaKeyNotFoundException()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(99, updated));
        }

        [Fact]
        public async Task UpdateAsync_ReemplazaTodosCampos()
        {
            var repository = CreateRepository();
            var updated = new Product 
            { 
                Nombre = "Laptop Completamente Nueva",
                Precio = 2000m,
                Calificacion = 5.0,
                Descripcion = "Descripcion totalmente nueva",
                UrlImagen = "https://newimg.com",
                Especificaciones = new() { { "GPU", "RTX 4090" } }
            };

            var result = await repository.UpdateAsync(2, updated);

            Assert.Equal(2, result.Id);
            Assert.Equal("Laptop Completamente Nueva", result.Nombre);
            Assert.Equal(2000m, result.Precio);
            Assert.Equal(5.0, result.Calificacion);
            Assert.Equal("https://newimg.com", result.UrlImagen);
        }

        [Fact]
        public async Task UpdateAsync_ErrorPersistencia_LanzaExcepcion()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            // Borrar el directorio para simular error de I/O
            Directory.Delete(_tempDirectory, true);
            
            // UpdateAsync debe lanzar excepción porque no puede escribir
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => repository.UpdateAsync(1, updated));
        }

        [Fact]
        public async Task UpdateAsync_IdCero_LanzaKeyNotFoundException()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            // ID 0 no existe
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(0, updated));
        }

        [Fact]
        public async Task UpdateAsync_IdNegativo_LanzaKeyNotFoundException()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            // ID negativo no existe
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(-1, updated));
        }

        // ?? PartialUpdateAsync ????????????????????????????????????????

        [Fact]
        public async Task PartialUpdateAsync_SoloPrecio_ActualizaSoloPrecio()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest { Precio = 599m };

            var result = await repository.PartialUpdateAsync(1, request);

            Assert.Equal(599m, result.Precio);
            Assert.Equal("Laptop Pro X1", result.Nombre);
            Assert.Equal(4.7, result.Calificacion);
        }

        [Fact]
        public async Task PartialUpdateAsync_IdNoExistente_LanzaKeyNotFoundException()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest { Precio = 599m };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.PartialUpdateAsync(99, request));
        }

        [Fact]
        public async Task PartialUpdateAsync_Multiplescampos_ActualizaTodos()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest 
            { 
                Nombre = "Laptop Actualizada",
                Precio = 1500m,
                Calificacion = 4.8
            };

            var result = await repository.PartialUpdateAsync(1, request);

            Assert.Equal("Laptop Actualizada", result.Nombre);
            Assert.Equal(1500m, result.Precio);
            Assert.Equal(4.8, result.Calificacion);
            // El resto mantiene sus valores originales
            Assert.Equal("Desc 1", result.Descripcion);
            Assert.Equal("https://img1.com", result.UrlImagen);
        }

        [Fact]
        public async Task PartialUpdateAsync_CamposNulos_NoModificaNada()
        {
            var repository = CreateRepository();
            var originalProduct = await repository.GetByIdAsync(1);
            var request = new UpdateProductRequest(); // Todos los campos nulos

            var result = await repository.PartialUpdateAsync(1, request);

            Assert.Equal(originalProduct!.Nombre, result.Nombre);
            Assert.Equal(originalProduct.Precio, result.Precio);
            Assert.Equal(originalProduct.Calificacion, result.Calificacion);
        }

        [Fact]
        public async Task PartialUpdateAsync_ErrorPersistencia_LanzaExcepcion()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest { Precio = 599m };

            // Borrar el directorio para simular error de I/O
            Directory.Delete(_tempDirectory, true);
            
            // PartialUpdateAsync debe lanzar excepción porque no puede escribir
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => repository.PartialUpdateAsync(1, request));
        }

        [Fact]
        public async Task PartialUpdateAsync_IdCero_LanzaKeyNotFoundException()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest { Precio = 599m };

            // ID 0 no existe
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.PartialUpdateAsync(0, request));
        }

        [Fact]
        public async Task PartialUpdateAsync_ActualizaEspecificaciones()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest 
            { 
                Especificaciones = new() { { "GPU", "NVIDIA RTX 4070" }, { "Memoria", "32GB" } }
            };

            var result = await repository.PartialUpdateAsync(1, request);

            Assert.NotNull(result.Especificaciones);
            Assert.Equal("NVIDIA RTX 4070", result.Especificaciones["GPU"]);
            Assert.Equal("32GB", result.Especificaciones["Memoria"]);
        }

        [Fact]
        public async Task PartialUpdateAsync_SoloNombre()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest { Nombre = "Nuevo Nombre" };

            var result = await repository.PartialUpdateAsync(3, request);

            Assert.Equal("Nuevo Nombre", result.Nombre);
            // Otros campos se mantienen
            Assert.Equal(1899.99m, result.Precio);
            Assert.Equal(4.9, result.Calificacion);
        }

        // ?? DeleteAsync ???????????????????????????????????????????????

        [Fact]
        public async Task DeleteAsync_IdExistente_RetornaTrue()
        {
            var repository = CreateRepository();
            var result = await repository.DeleteAsync(1);
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_IdExistente_EliminaDelDiccionario()
        {
            var repository = CreateRepository();
            
            await repository.DeleteAsync(1);
            
            var deleted = await repository.GetByIdAsync(1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_IdNoExistente_RetornaFalse()
        {
            var repository = CreateRepository();
            var result = await repository.DeleteAsync(99);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_MultipleProductos_EliminaCorrectamente()
        {
            var repository = CreateRepository();
            var allBefore = await repository.GetAllAsync();
            
            await repository.DeleteAsync(1);
            await repository.DeleteAsync(2);
            
            var allAfter = await repository.GetAllAsync();
            
            Assert.Equal(allBefore.Count - 2, allAfter.Count);
            Assert.DoesNotContain(allAfter, p => p.Id == 1);
            Assert.DoesNotContain(allAfter, p => p.Id == 2);
            Assert.Contains(allAfter, p => p.Id == 3);
        }

        [Fact]
        public async Task DeleteAsync_ProductoNoExistente_NoCambiaConteo()
        {
            var repository = CreateRepository();
            var countBefore = (await repository.GetAllAsync()).Count;
            
            var result = await repository.DeleteAsync(999);
            
            var countAfter = (await repository.GetAllAsync()).Count;
            
            Assert.False(result);
            Assert.Equal(countBefore, countAfter);
        }

        [Fact]
        public async Task DeleteAsync_ErrorPersistencia_LanzaExcepcion()
        {
            var repository = CreateRepository();
            
            // Borrar el directorio para simular error de I/O
            Directory.Delete(_tempDirectory, true);
            
            // DeleteAsync debe lanzar excepción porque no puede escribir
            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => repository.DeleteAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_IdCero_RetornaFalse()
        {
            var repository = CreateRepository();
            var result = await repository.DeleteAsync(0);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_IdNegativo_RetornaFalse()
        {
            var repository = CreateRepository();
            var result = await repository.DeleteAsync(-1);
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_EliminaTodosLosProductos()
        {
            var repository = CreateRepository();
            
            await repository.DeleteAsync(1);
            await repository.DeleteAsync(2);
            await repository.DeleteAsync(3);
            
            var allProducts = await repository.GetAllAsync();
            Assert.Empty(allProducts);
        }

        [Fact]
        public async Task DeleteAsync_PersisteCambiosEnArchivo()
        {
            var repository = CreateRepository();
            
            await repository.DeleteAsync(1);
            
            // Crear nuevo repositorio para verificar que se guardó en archivo
            var newRepository = CreateRepository();
            var allProducts = await newRepository.GetAllAsync();
            
            Assert.Equal(2, allProducts.Count);
            Assert.DoesNotContain(allProducts, p => p.Id == 1);
        }

        // ?? IsHealthyAsync ????????????????????????????????????????????

        [Fact]
        public async Task IsHealthyAsync_ArchivoExisteYJsonValido_RetornaTrue()
        {
            var repository = CreateRepository();
            var result = await repository.IsHealthyAsync();
            Assert.True(result);
        }

        [Fact]
        public async Task IsHealthyAsync_ArchivoNoExiste_RetornaFalse()
        {
            var repository = CreateRepository();
            
            // Borrar el archivo JSON
            File.Delete(_testJsonPath);
            
            var result = await repository.IsHealthyAsync();
            Assert.False(result);
        }

        [Fact]
        public async Task IsHealthyAsync_JsonCorrompido_RetornaFalse()
        {
            var repository = CreateRepository();
            
            // Escribir JSON inválido
            File.WriteAllText(_testJsonPath, "{ esto no es json valido [[[");
            
            // IsHealthyAsync debe retornar False sin lanzar excepción
            var result = await repository.IsHealthyAsync();
            
            Assert.False(result);
        }

        // ?? Constructor ????????????????????????????????????????????????

        [Fact]
        public void Constructor_ArchivoNoExiste_LanzaFileNotFoundException()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"NonExistentTest_{Guid.NewGuid()}");
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["DATA_PATH"]).Returns(tempDir);
            
            var fileReader = new JsonFileReader(mockConfig.Object);
            
            // El constructor debe lanzar FileNotFoundException
            Assert.Throws<FileNotFoundException>(() =>
                new ProductRepository(_mockLogger.Object, _mockEnv.Object, fileReader));
        }

        [Fact]
        public void Constructor_JsonInvalido_LanzaJsonException()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"JsonErrorTest_{Guid.NewGuid()}");
            var dataFolder = Path.Combine(tempDir, "Data");
            Directory.CreateDirectory(dataFolder);
            var jsonPath = Path.Combine(dataFolder, "products.json");
            
            // Escribir JSON inválido
            File.WriteAllText(jsonPath, "{ esto no es json valido [[[");
            
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["DATA_PATH"]).Returns(tempDir);
            
            var fileReader = new JsonFileReader(mockConfig.Object);
            
            // El constructor debe lanzar JsonException
            Assert.Throws<JsonException>(() =>
                new ProductRepository(_mockLogger.Object, _mockEnv.Object, fileReader));
            
            // Limpiar
            Directory.Delete(tempDir, true);
        }

        [Fact]
        public void Constructor_CargaProductosCorrectamente()
        {
            var repository = CreateRepository();
            
            // El repositorio debe tener cargados los 3 productos
            var result = repository.GetAllAsync().Result;
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public void Constructor_AsignaJsonPathCorrectamente()
        {
            var mockConfig = new Mock<IConfiguration>();
            mockConfig.Setup(c => c["DATA_PATH"]).Returns(_tempDirectory);
            
            var fileReader = new JsonFileReader(mockConfig.Object);
            
            // Verificar que JsonPath tiene la estructura esperada
            Assert.Contains("Data", fileReader.JsonPath);
            Assert.EndsWith("products.json", fileReader.JsonPath);
        }
    }
}
