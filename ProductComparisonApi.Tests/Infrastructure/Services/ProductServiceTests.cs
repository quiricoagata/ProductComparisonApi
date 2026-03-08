using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;
using ProductComparisonApi.Infrastructure.Services;
using System.Text.Json;

namespace ProductComparisonApi.Tests.Infrastructure.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IJsonFileReader> _mockFileReader;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        private readonly List<Product> _fakeProducts;
        private readonly string _fakePath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ProductServiceTests()
        {
            _mockFileReader = new();
            _mockLogger = new();
            _mockEnv = new();

            _fakeProducts = new()
            {
                new() { Id = 1, Nombre = "Laptop Pro X1", Precio = 1299.99m, Calificacion = 4.7, Descripcion = "Desc 1", UrlImagen = "https://img1.com", Especificaciones = new() { { "RAM", "16GB" } } },
                new() { Id = 2, Nombre = "Laptop UltraSlim", Precio = 999.99m, Calificacion = 4.3, Descripcion = "Desc 2", UrlImagen = "https://img2.com", Especificaciones = new() { { "RAM", "8GB" } } },
                new() { Id = 3, Nombre = "Laptop Gaming", Precio = 1899.99m, Calificacion = 4.9, Descripcion = "Desc 3", UrlImagen = "https://img3.com", Especificaciones = new() { { "RAM", "32GB" } } }
            };

            _mockEnv.Setup(e => e.ContentRootPath).Returns("");
            _fakePath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");

            _mockFileReader.Setup(f => f.FileExists(_fakePath)).Returns(true);
            _mockFileReader.Setup(f => f.ReadAllText(_fakePath))
                .Returns(JsonSerializer.Serialize(_fakeProducts, _jsonOptions));

            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        private ProductService CreateService()
        {
            var mockConfig = new Mock<IConfiguration>();
            return new ProductService(_mockLogger.Object, _mockEnv.Object, _mockFileReader.Object, mockConfig.Object);
        }


        [Fact]
        public async Task GetAllAsync_ExistenProductos_RetornaTodosLosProductos()
        {
            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetAllAsync_ExistenProductos_RetornaListaOrdenadaPorId()
        {
            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
            Assert.Equal(3, result[2].Id);
        }

        // ── GetByIdAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_IdExistente_RetornaProductoCorrecto()
        {
            var service = CreateService();

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Laptop Pro X1", result.Nombre);
        }

        [Fact]
        public async Task GetByIdAsync_IdNoExistente_RetornaNull()
        {
            var service = CreateService();

            var result = await service.GetByIdAsync(99);

            Assert.Null(result);
        }

        // ── GetByIdsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetByIdsAsync_IdsExistentes_RetornaProductosCorrectos()
        {
            var service = CreateService();

            var result = await service.GetByIdsAsync(new List<int> { 1, 3 });

            Assert.Equal(2, result.Count);
            Assert.Contains(result, p => p.Id == 1);
            Assert.Contains(result, p => p.Id == 3);
        }

        [Fact]
        public async Task GetByIdsAsync_AlgunIdNoExistente_RetornaSoloLosQueExisten()
        {
            var service = CreateService();

            var result = await service.GetByIdsAsync(new List<int> { 1, 99 });

            Assert.Single(result);
            Assert.Equal(1, result[0].Id);
        }

        [Fact]
        public async Task GetByIdsAsync_NingunIdExistente_RetornaListaVacia()
        {
            var service = CreateService();

            var result = await service.GetByIdsAsync(new List<int> { 98, 99 });

            Assert.Empty(result);
        }

        // ── CreateAsync ───────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_ProductoValido_RetornaProductoConIdGenerado()
        {
            var service = CreateService();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            var result = await service.CreateAsync(newProduct);

            Assert.Equal(4, result.Id);
            Assert.Equal("Laptop Nueva", result.Nombre);
        }

        [Fact]
        public async Task CreateAsync_ProductoValido_SeAgregaAlDiccionario()
        {
            var service = CreateService();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await service.CreateAsync(newProduct);
            var allProducts = await service.GetAllAsync();

            // Verifica que el ConcurrentDictionary tiene el nuevo producto
            Assert.Equal(4, allProducts.Count);
            Assert.Contains(allProducts, p => p.Nombre == "Laptop Nueva");
        }

        [Fact]
        public async Task CreateAsync_ProductosCreados_IdsIncrementanSinRepetirse()
        {
            var service = CreateService();

            // Crea tres productos seguidos y verifica que los IDs no se repiten
            var p1 = await service.CreateAsync(new Product { Nombre = "A", Precio = 100m, Calificacion = 4.0, Descripcion = "D", UrlImagen = "https://img.com" });
            var p2 = await service.CreateAsync(new Product { Nombre = "B", Precio = 200m, Calificacion = 4.0, Descripcion = "D", UrlImagen = "https://img.com" });
            var p3 = await service.CreateAsync(new Product { Nombre = "C", Precio = 300m, Calificacion = 4.0, Descripcion = "D", UrlImagen = "https://img.com" });

            var ids = new[] { p1.Id, p2.Id, p3.Id };
            Assert.Equal(ids.Distinct().Count(), ids.Length);
        }

        [Fact]
        public async Task CreateAsync_ProductoValido_PersisteCambiosEnJson()
        {
            var service = CreateService();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await service.CreateAsync(newProduct);

            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_VariasOperacionesConcurrentes_NoGeneraIdsDuplicados()
        {
            var service = CreateService();

            // Simula múltiples creates concurrentes — el ConcurrentDictionary debe evitar IDs duplicados
            var tasks = Enumerable.Range(0, 10).Select(_ =>
                service.CreateAsync(new Product
                {
                    Nombre = "Laptop Concurrente",
                    Precio = 999m,
                    Calificacion = 4.0,
                    Descripcion = "Desc",
                    UrlImagen = "https://img.com"
                }));

            var results = await Task.WhenAll(tasks);
            var ids = results.Select(p => p.Id).ToList();

            // Todos los IDs deben ser únicos
            Assert.Equal(ids.Distinct().Count(), ids.Count);
        }

        // ── UpdateAsync ───────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_IdExistente_RetornaProductoActualizado()
        {
            var service = CreateService();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            var result = await service.UpdateAsync(1, updated);

            Assert.Equal(1, result.Id);
            Assert.Equal("Laptop Actualizada", result.Nombre);
            Assert.Equal(1099m, result.Precio);
        }

        [Fact]
        public async Task UpdateAsync_IdExistente_ActualizaElDiccionario()
        {
            var service = CreateService();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            await service.UpdateAsync(1, updated);

            // Verifica que el ConcurrentDictionary refleja el cambio
            var fromDictionary = await service.GetByIdAsync(1);
            Assert.Equal("Laptop Actualizada", fromDictionary!.Nombre);
        }

        [Fact]
        public async Task UpdateAsync_IdNoExistente_LanzaKeyNotFoundException()
        {
            var service = CreateService();
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(99, updated));
        }

        // ── PartialUpdateAsync ────────────────────────────────────────

        [Fact]
        public async Task PartialUpdateAsync_SoloPrecio_ActualizaSoloPrecio()
        {
            var service = CreateService();
            var request = new UpdateProductRequest { Precio = 599m };

            var result = await service.PartialUpdateAsync(1, request);

            Assert.Equal(599m, result.Precio);
            Assert.Equal("Laptop Pro X1", result.Nombre);
            Assert.Equal(4.7, result.Calificacion);
        }

        [Fact]
        public async Task PartialUpdateAsync_IdNoExistente_LanzaKeyNotFoundException()
        {
            var service = CreateService();
            var request = new UpdateProductRequest { Precio = 599m };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.PartialUpdateAsync(99, request));
        }

        [Fact]
        public async Task PartialUpdateAsync_CamposNulos_NoModificaNada()
        {
            var service = CreateService();
            var request = new UpdateProductRequest();
            var original = await service.GetByIdAsync(1);
            var originalNombre = original!.Nombre;
            var originalPrecio = original!.Precio;

            var result = await service.PartialUpdateAsync(1, request);

            Assert.Equal(originalNombre, result.Nombre);
            Assert.Equal(originalPrecio, result.Precio);
        }

        [Fact]
        public async Task PartialUpdateAsync_IdExistente_ActualizaElDiccionario()
        {
            var service = CreateService();
            var request = new UpdateProductRequest { Precio = 599m };

            await service.PartialUpdateAsync(1, request);

            // Verifica que el ConcurrentDictionary refleja el cambio parcial
            var fromDictionary = await service.GetByIdAsync(1);
            Assert.Equal(599m, fromDictionary!.Precio);
        }

        // ── DeleteAsync ───────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_IdExistente_RetornaTrue()
        {
            var service = CreateService();

            var result = await service.DeleteAsync(1);

            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_IdExistente_EliminaDelDiccionario()
        {
            var service = CreateService();

            await service.DeleteAsync(1);

            // Verifica que el ConcurrentDictionary ya no contiene el producto eliminado
            var deleted = await service.GetByIdAsync(1);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteAsync_IdExistente_ReduceElConteoDelDiccionario()
        {
            var service = CreateService();

            await service.DeleteAsync(1);
            var allProducts = await service.GetAllAsync();

            Assert.Equal(2, allProducts.Count);
            Assert.DoesNotContain(allProducts, p => p.Id == 1);
        }

        [Fact]
        public async Task DeleteAsync_IdNoExistente_RetornaFalse()
        {
            var service = CreateService();

            var result = await service.DeleteAsync(99);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_IdNoExistente_NoEscribeElJson()
        {
            var service = CreateService();

            await service.DeleteAsync(99);

            // Si el producto no existe no hay nada que persistir
            // El SemaphoreSlim nunca debería haberse adquirido
            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_IdExistente_PersisteCambiosEnJson()
        {
            var service = CreateService();

            await service.DeleteAsync(1);

            // Verifica que el SemaphoreSlim liberó el acceso y se escribió el archivo
            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        // ── IsHealthyAsync ────────────────────────────────────────────

        [Fact]
        public async Task IsHealthyAsync_ArchivoExisteYJsonValido_RetornaTrue()
        {
            var service = CreateService();

            var result = await service.IsHealthyAsync();

            Assert.True(result);
        }

        [Fact]
        public async Task IsHealthyAsync_ArchivoNoExiste_RetornaFalse()
        {
            // Creamos el servicio con la configuración por defecto (archivo existe)
            var service = CreateService();

            // Simulamos que el archivo deja de existir después de la inicialización
            _mockFileReader.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            var result = await service.IsHealthyAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task IsHealthyAsync_JsonCorrompido_RetornaFalse()
        {
            var service = CreateService();

            // Simula que el archivo fue corrompido después de inicializar el servicio
            _mockFileReader.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("{ json corrompido [[[");

            var result = await service.IsHealthyAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task IsHealthyAsync_ErrorAlLeerArchivo_RetornaFalse()
        {
            var service = CreateService();

            // Simula un error de I/O al leer el archivo
            _mockFileReader.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new IOException("Error de lectura."));

            var result = await service.IsHealthyAsync();

            Assert.False(result);
        }

        // ── Inicialización ────────────────────────────────────────────

        [Fact]
        public void Constructor_ArchivoNoExiste_LanzaFileNotFoundException()
        {
            _mockFileReader.Setup(f => f.FileExists(_fakePath)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => CreateService());
        }

        [Fact]
        public async Task Constructor_CargaProductosEnElDiccionario_CantidadCorrecta()
        {
            var service = CreateService();

            // Verifica que el ConcurrentDictionary se pobló correctamente desde el JSON
            var result = await service.GetAllAsync();
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task Constructor_CargaProductosEnElDiccionario_IdsComoClaves()
        {
            var service = CreateService();

            // Verifica que los IDs del JSON se usaron como claves del diccionario
            // comprobando que la búsqueda por ID funciona correctamente
            var p1 = await service.GetByIdAsync(1);
            var p2 = await service.GetByIdAsync(2);
            var p3 = await service.GetByIdAsync(3);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);
        }
    }
}