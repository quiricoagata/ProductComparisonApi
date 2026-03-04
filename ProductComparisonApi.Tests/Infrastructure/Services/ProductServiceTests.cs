using Microsoft.AspNetCore.Hosting;
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

        // Lista de productos que simula el contenido del JSON
        private readonly List<Product> _fakeProducts;
        private readonly string _fakePath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ProductServiceTests()
        {
            _mockFileReader = new Mock<IJsonFileReader>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            _mockEnv = new Mock<IWebHostEnvironment>();

            _fakeProducts = new List<Product>
            {
                new() { Id = 1, Nombre = "Laptop Pro X1", Precio = 1299.99m, Calificacion = 4.7, Descripcion = "Desc 1", UrlImagen = "https://img1.com", Especificaciones = new() { { "RAM", "16GB" } } },
                new() { Id = 2, Nombre = "Laptop UltraSlim", Precio = 999.99m, Calificacion = 4.3, Descripcion = "Desc 2", UrlImagen = "https://img2.com", Especificaciones = new() { { "RAM", "8GB" } } },
                new() { Id = 3, Nombre = "Laptop Gaming", Precio = 1899.99m, Calificacion = 4.9, Descripcion = "Desc 3", UrlImagen = "https://img3.com", Especificaciones = new() { { "RAM", "32GB" } } }
            };

            // Configuramos el entorno simulado
            _mockEnv.Setup(e => e.ContentRootPath).Returns("");
            
            // Construir la ruta exactamente como ProductService lo hace
            _fakePath = Path.Combine("", "Data", "products.json");
            
            _mockFileReader.Setup(f => f.FileExists(_fakePath)).Returns(true);
            _mockFileReader.Setup(f => f.ReadAllText(_fakePath))
                .Returns(JsonSerializer.Serialize(_fakeProducts, _jsonOptions));

            // WriteAllTextAsync no necesita hacer nada real en los tests
            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        // Método helper para crear una instancia fresca del servicio en cada test
        private ProductService CreateService() =>
            new ProductService(_mockLogger.Object, _mockEnv.Object, _mockFileReader.Object);

        // ── GetAllAsync ───────────────────────────────────────────────

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

            // El ID generado debe ser el máximo existente + 1 (3 + 1 = 4)
            Assert.Equal(4, result.Id);
            Assert.Equal("Laptop Nueva", result.Nombre);
        }

        [Fact]
        public async Task CreateAsync_ProductoValido_SeAgregaALaLista()
        {
            var service = CreateService();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await service.CreateAsync(newProduct);
            var allProducts = await service.GetAllAsync();

            Assert.Equal(4, allProducts.Count);
        }

        [Fact]
        public async Task CreateAsync_ProductoValido_PersisteCambiosEnJson()
        {
            var service = CreateService();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await service.CreateAsync(newProduct);

            // Verificamos que se llamó a WriteAllTextAsync para persistir (sin validar la ruta exacta)
            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
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
        public async Task UpdateAsync_IdNoExistente_LanzaKeyNotFoundException()
        {
            var service = CreateService();
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(99, updated));
        }

        [Fact]
        public async Task UpdateAsync_IdExistente_PersisteCambiosEnJson()
        {
            var service = CreateService();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            await service.UpdateAsync(1, updated);

            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        // ── PartialUpdateAsync ────────────────────────────────────────

        [Fact]
        public async Task PartialUpdateAsync_SoloPrecio_ActualizaSoloPrecio()
        {
            var service = CreateService();
            var request = new UpdateProductRequest { Precio = 599m };

            var result = await service.PartialUpdateAsync(1, request);

            // El precio cambia
            Assert.Equal(599m, result.Precio);

            // Los demás campos se conservan
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

            // Request vacío — ningún campo viene en el body
            var request = new UpdateProductRequest();
            var original = await service.GetByIdAsync(1);
            var originalNombre = original!.Nombre;
            var originalPrecio = original!.Precio;

            var result = await service.PartialUpdateAsync(1, request);

            Assert.Equal(originalNombre, result.Nombre);
            Assert.Equal(originalPrecio, result.Precio);
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
        public async Task DeleteAsync_IdExistente_SeEliminaDeLaLista()
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
        public async Task DeleteAsync_IdExistente_PersisteCambiosEnJson()
        {
            var service = CreateService();

            await service.DeleteAsync(1);

            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        // ── Inicialización ────────────────────────────────────────────

        [Fact]
        public void Constructor_ArchivoNoExiste_LanzaFileNotFoundException()
        {
            _mockFileReader.Setup(f => f.FileExists(_fakePath)).Returns(false);

            Assert.Throws<FileNotFoundException>(() => CreateService());
        }
    }
}