using Microsoft.Extensions.Logging;
using Moq;
using ProductComparisonApi.Application.Services;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Tests.Application.Services
{
    public class ProductServiceTests
    {
        private readonly Mock<IProductRepository> _mockRepository;
        private readonly Mock<ILogger<ProductService>> _mockLogger;

        private readonly List<Product> _fakeProducts;

        public ProductServiceTests()
        {
            _mockRepository = new();
            _mockLogger = new();

            _fakeProducts = new()
            {
                new() { Id = 1, Nombre = "Laptop Pro X1", Precio = 1299.99m, Calificacion = 4.7, Descripcion = "Desc 1", UrlImagen = "https://img1.com" },
                new() { Id = 2, Nombre = "Laptop UltraSlim", Precio = 999.99m, Calificacion = 4.3, Descripcion = "Desc 2", UrlImagen = "https://img2.com" },
                new() { Id = 3, Nombre = "Laptop Gaming", Precio = 1899.99m, Calificacion = 4.9, Descripcion = "Desc 3", UrlImagen = "https://img3.com" }
            };
        }

        private ProductService CreateService() =>
            new ProductService(_mockRepository.Object, _mockLogger.Object);

        // ── GetAllAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_DelegaAlRepository_RetornaResultado()
        {
            _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(_fakeProducts);
            var service = CreateService();

            var result = await service.GetAllAsync();

            Assert.Equal(3, result.Count);
            _mockRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_RepositoryLanzaExcepcion_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new Exception("Error"));
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(() => service.GetAllAsync());
        }

        // ── GetByIdAsync ──────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_DelegaAlRepository_RetornaProducto()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(_fakeProducts[0]);
            var service = CreateService();

            var result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            _mockRepository.Verify(r => r.GetByIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_IdNoExistente_RetornaNull()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Product?)null);
            var service = CreateService();

            var result = await service.GetByIdAsync(99);

            Assert.Null(result);
        }

        // ── GetByIdsAsync ─────────────────────────────────────────────

        [Fact]
        public async Task GetByIdsAsync_DelegaAlRepository_RetornaProductos()
        {
            var ids = new List<int> { 1, 2 };
            _mockRepository.Setup(r => r.GetByIdsAsync(ids)).ReturnsAsync(_fakeProducts.Take(2).ToList());
            var service = CreateService();

            var result = await service.GetByIdsAsync(ids);

            Assert.Equal(2, result.Count);
            _mockRepository.Verify(r => r.GetByIdsAsync(ids), Times.Once);
        }

        // ── CreateAsync ───────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_DelegaAlRepository_RetornaProductoCreado()
        {
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };
            var createdProduct = new Product { Id = 4, Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.CreateAsync(newProduct)).ReturnsAsync(createdProduct);
            var service = CreateService();

            var result = await service.CreateAsync(newProduct);

            Assert.Equal(4, result.Id);
            _mockRepository.Verify(r => r.CreateAsync(newProduct), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_RepositoryLanzaExcepcion_PropagaExcepcion()
        {
            var newProduct = new Product { Nombre = "Laptop", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.CreateAsync(newProduct)).ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.CreateAsync(newProduct));
        }

        // ── UpdateAsync ───────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_DelegaAlRepository_RetornaProductoActualizado()
        {
            var updated = new Product { Nombre = "Actualizado", Precio = 1099m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };
            var updatedWithId = new Product { Id = 1, Nombre = "Actualizado", Precio = 1099m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.UpdateAsync(1, updated)).ReturnsAsync(updatedWithId);
            var service = CreateService();

            var result = await service.UpdateAsync(1, updated);

            Assert.Equal(1, result.Id);
            Assert.Equal("Actualizado", result.Nombre);
            _mockRepository.Verify(r => r.UpdateAsync(1, updated), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_IdNoExistente_PropagaKeyNotFoundException()
        {
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.UpdateAsync(99, updated)).ThrowsAsync(new KeyNotFoundException());
            var service = CreateService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateAsync(99, updated));
        }

        // ── PartialUpdateAsync ────────────────────────────────────────

        [Fact]
        public async Task PartialUpdateAsync_DelegaAlRepository_RetornaProductoActualizado()
        {
            var request = new UpdateProductRequest { Precio = 599m };
            var updatedProduct = new Product { Id = 1, Nombre = "Laptop Pro X1", Precio = 599m, Calificacion = 4.7, Descripcion = "Desc 1", UrlImagen = "https://img1.com" };
            _mockRepository.Setup(r => r.PartialUpdateAsync(1, request)).ReturnsAsync(updatedProduct);
            var service = CreateService();

            var result = await service.PartialUpdateAsync(1, request);

            Assert.Equal(599m, result.Precio);
            _mockRepository.Verify(r => r.PartialUpdateAsync(1, request), Times.Once);
        }

        [Fact]
        public async Task PartialUpdateAsync_IdNoExistente_PropagaKeyNotFoundException()
        {
            var request = new UpdateProductRequest { Precio = 599m };
            _mockRepository.Setup(r => r.PartialUpdateAsync(99, request)).ThrowsAsync(new KeyNotFoundException());
            var service = CreateService();

            await Assert.ThrowsAsync<KeyNotFoundException>(() => service.PartialUpdateAsync(99, request));
        }

        // ── DeleteAsync ───────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_DelegaAlRepository_RetornaTrue()
        {
            _mockRepository.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
            var service = CreateService();

            var result = await service.DeleteAsync(1);

            Assert.True(result);
            _mockRepository.Verify(r => r.DeleteAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_IdNoExistente_RetornaFalse()
        {
            _mockRepository.Setup(r => r.DeleteAsync(99)).ReturnsAsync(false);
            var service = CreateService();

            var result = await service.DeleteAsync(99);

            Assert.False(result);
        }

        // ── IsHealthyAsync ────────────────────────────────────────────

        [Fact]
        public async Task IsHealthyAsync_DelegaAlRepository_RetornaTrue()
        {
            _mockRepository.Setup(r => r.IsHealthyAsync()).ReturnsAsync(true);
            var service = CreateService();

            var result = await service.IsHealthyAsync();

            Assert.True(result);
            _mockRepository.Verify(r => r.IsHealthyAsync(), Times.Once);
        }

        [Fact]
        public async Task IsHealthyAsync_RepositoryRetornaFalse_RetornaFalse()
        {
            _mockRepository.Setup(r => r.IsHealthyAsync()).ReturnsAsync(false);
            var service = CreateService();

            var result = await service.IsHealthyAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task IsHealthyAsync_RepositoryLanzaExcepcion_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.IsHealthyAsync()).ThrowsAsync(new Exception("Error"));
            var service = CreateService();

            await Assert.ThrowsAsync<Exception>(() => service.IsHealthyAsync());
        }

        // ── GetAllAsync - Excepciones ─────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.GetAllAsync()).ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.GetAllAsync());
        }

        // ── GetByIdAsync - Excepciones ────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(1)).ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.GetByIdAsync(1));
        }

        // ── GetByIdsAsync - Excepciones ───────────────────────────────────────

        [Fact]
        public async Task GetByIdsAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            var ids = new List<int> { 1, 2 };
            _mockRepository.Setup(r => r.GetByIdsAsync(ids)).ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.GetByIdsAsync(ids));
        }

        // ── CreateAsync - Excepciones ─────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_RepositoryLanzaUnauthorizedAccessException_PropagaExcepcion()
        {
            var newProduct = new Product { Nombre = "Laptop", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.CreateAsync(newProduct))
                .ThrowsAsync(new UnauthorizedAccessException("Acceso denegado"));
            var service = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateAsync(newProduct));
        }

        [Fact]
        public async Task CreateAsync_RepositoryLanzaDirectoryNotFoundException_PropagaExcepcion()
        {
            var newProduct = new Product { Nombre = "Laptop", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.CreateAsync(newProduct))
                .ThrowsAsync(new DirectoryNotFoundException("Directorio no encontrado"));
            var service = CreateService();

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => service.CreateAsync(newProduct));
        }

        // ── UpdateAsync - Excepciones ─────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.UpdateAsync(1, updated))
                .ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.UpdateAsync(1, updated));
        }

        [Fact]
        public async Task UpdateAsync_RepositoryLanzaDirectoryNotFoundException_PropagaExcepcion()
        {
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };
            _mockRepository.Setup(r => r.UpdateAsync(1, updated))
                .ThrowsAsync(new DirectoryNotFoundException("Directorio no encontrado"));
            var service = CreateService();

            await Assert.ThrowsAsync<DirectoryNotFoundException>(() => service.UpdateAsync(1, updated));
        }

        // ── PartialUpdateAsync - Excepciones ──────────────────────────────────

        [Fact]
        public async Task PartialUpdateAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            var request = new UpdateProductRequest { Precio = 599m };
            _mockRepository.Setup(r => r.PartialUpdateAsync(1, request))
                .ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.PartialUpdateAsync(1, request));
        }

        [Fact]
        public async Task PartialUpdateAsync_RepositoryLanzaNotSupportedException_PropagaExcepcion()
        {
            var request = new UpdateProductRequest { Precio = 599m };
            _mockRepository.Setup(r => r.PartialUpdateAsync(1, request))
                .ThrowsAsync(new NotSupportedException("Operación no soportada"));
            var service = CreateService();

            await Assert.ThrowsAsync<NotSupportedException>(() => service.PartialUpdateAsync(1, request));
        }

        // ── DeleteAsync - Excepciones ─────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.DeleteAsync(1))
                .ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.DeleteAsync(1));
        }

        [Fact]
        public async Task DeleteAsync_RepositoryLanzaTimeoutException_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.DeleteAsync(1))
                .ThrowsAsync(new TimeoutException("Tiempo de espera excedido"));
            var service = CreateService();

            await Assert.ThrowsAsync<TimeoutException>(() => service.DeleteAsync(1));
        }

        // ── IsHealthyAsync - Excepciones ──────────────────────────────────────

        [Fact]
        public async Task IsHealthyAsync_RepositoryLanzaIOException_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.IsHealthyAsync())
                .ThrowsAsync(new IOException("Error de I/O"));
            var service = CreateService();

            await Assert.ThrowsAsync<IOException>(() => service.IsHealthyAsync());
        }

        [Fact]
        public async Task IsHealthyAsync_RepositoryLanzaUnauthorizedAccessException_PropagaExcepcion()
        {
            _mockRepository.Setup(r => r.IsHealthyAsync())
                .ThrowsAsync(new UnauthorizedAccessException("Acceso denegado"));
            var service = CreateService();

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.IsHealthyAsync());
        }
    }
}