using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;
using ProductComparisonApi.Infrastructure.Repositories;
using ProductComparisonApi.Tests.Application.Services;
using ProductComparisonApi.Tests.Controllers;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace ProductComparisonApi.Tests.Infrastructure.Repositories
{
    public class ProductRepositoryTests
    {
        private readonly Mock<IJsonFileReader> _mockFileReader;
        private readonly Mock<ILogger<ProductRepository>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnv;

        private readonly List<Product> _fakeProducts;
        private readonly string _fakePath;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ProductRepositoryTests()
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

            _fakePath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json");

            _mockFileReader.Setup(f => f.JsonPath).Returns(_fakePath);
            _mockFileReader.Setup(f => f.FileExists(_fakePath)).Returns(true);
            _mockFileReader.Setup(f => f.ReadAllText(_fakePath))
                .Returns(JsonSerializer.Serialize(_fakeProducts, _jsonOptions));
            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(Task.CompletedTask);
        }

        private ProductRepository CreateRepository() =>
            new ProductRepository(_mockLogger.Object, _mockEnv.Object, _mockFileReader.Object);

        // ── GetAllAsync ───────────────────────────────────────────────

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

        // ── GetByIdAsync ──────────────────────────────────────────────

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
        public async Task GetByIdAsync_IdMuyGrande_RetornaNull()
        {
            var repository = CreateRepository();

            var result = await repository.GetByIdAsync(int.MaxValue);

            Assert.Null(result);
        }

        // ── GetByIdsAsync ─────────────────────────────────────────────

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

        // ── CreateAsync ───────────────────────────────────────────────

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
        public async Task CreateAsync_ProductoValido_SeAgregaAlDiccionario()
        {
            var repository = CreateRepository();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await repository.CreateAsync(newProduct);
            var allProducts = await repository.GetAllAsync();

            Assert.Equal(4, allProducts.Count);
            Assert.Contains(allProducts, p => p.Nombre == "Laptop Nueva");
        }

        [Fact]
        public async Task CreateAsync_ProductoValido_PersisteCambiosEnJson()
        {
            var repository = CreateRepository();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await repository.CreateAsync(newProduct);

            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_VariasOperacionesConcurrentes_NoGeneraIdsDuplicados()
        {
            var repository = CreateRepository();

            var tasks = Enumerable.Range(0, 10).Select(_ =>
                repository.CreateAsync(new Product
                {
                    Nombre = "Laptop Concurrente",
                    Precio = 999m,
                    Calificacion = 4.0,
                    Descripcion = "Desc",
                    UrlImagen = "https://img.com"
                }));

            var results = await Task.WhenAll(tasks);
            var ids = results.Select(p => p.Id).ToList();

            Assert.Equal(ids.Distinct().Count(), ids.Count);
        }

        [Fact]
        public async Task CreateAsync_ErrorAlPersistir_LanzaExcepcion()
        {
            var repository = CreateRepository();
            var newProduct = new Product { Nombre = "Laptop Nueva", Precio = 799m, Calificacion = 4.0, Descripcion = "Desc", UrlImagen = "https://img.com" };

            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IOException("Error de I/O"));

            await Assert.ThrowsAsync<IOException>(() => repository.CreateAsync(newProduct));
        }

        // ── UpdateAsync ───────────────────────────────────────────────

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
        }

        [Fact]
        public async Task UpdateAsync_IdNoExistente_LanzaKeyNotFoundException()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop", Precio = 999m, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.UpdateAsync(99, updated));
        }

        [Fact]
        public async Task UpdateAsync_ErrorAlPersistir_LanzaExcepcion()
        {
            var repository = CreateRepository();
            var updated = new Product { Nombre = "Laptop Actualizada", Precio = 1099m, Calificacion = 4.5, Descripcion = "Nueva desc", UrlImagen = "https://nuevaimg.com" };

            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IOException("Error de I/O"));

            await Assert.ThrowsAsync<IOException>(() => repository.UpdateAsync(1, updated));
        }

        // ── PartialUpdateAsync ────────────────────────────────────────

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
        public async Task PartialUpdateAsync_CamposNulos_NoModificaNada()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest();
            var original = await repository.GetByIdAsync(1);

            var result = await repository.PartialUpdateAsync(1, request);

            Assert.Equal(original!.Nombre, result.Nombre);
            Assert.Equal(original.Precio, result.Precio);
        }

        [Fact]
        public async Task PartialUpdateAsync_ErrorAlPersistir_LanzaExcepcion()
        {
            var repository = CreateRepository();
            var request = new UpdateProductRequest { Precio = 599m };

            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IOException("Error al escribir"));

            await Assert.ThrowsAsync<IOException>(() => repository.PartialUpdateAsync(1, request));
        }

        // ── DeleteAsync ───────────────────────────────────────────────

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
        public async Task DeleteAsync_IdNoExistente_NoEscribeElJson()
        {
            var repository = CreateRepository();

            await repository.DeleteAsync(99);

            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_IdExistente_PersisteCambiosEnJson()
        {
            var repository = CreateRepository();

            await repository.DeleteAsync(1);

            _mockFileReader.Verify(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ErrorAlPersistir_LanzaExcepcion()
        {
            var repository = CreateRepository();

            _mockFileReader.Setup(f => f.WriteAllTextAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new IOException("Error de I/O"));

            await Assert.ThrowsAsync<IOException>(() => repository.DeleteAsync(1));
        }

        // ── IsHealthyAsync ────────────────────────────────────────────

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
            _mockFileReader.Setup(f => f.FileExists(It.IsAny<string>())).Returns(false);

            var result = await repository.IsHealthyAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task IsHealthyAsync_JsonCorrompido_RetornaFalse()
        {
            var repository = CreateRepository();
            _mockFileReader.Setup(f => f.ReadAllText(It.IsAny<string>())).Returns("{ json corrompido [[[");

            var result = await repository.IsHealthyAsync();

            Assert.False(result);
        }

        [Fact]
        public async Task IsHealthyAsync_ErrorAlLeerArchivo_RetornaFalse()
        {
            var repository = CreateRepository();
            _mockFileReader.Setup(f => f.ReadAllText(It.IsAny<string>())).Throws(new IOException("Error de lectura."));

            var result = await repository.IsHealthyAsync();

            Assert.False(result);
        }

        // ── Constructor ───────────────────────────────────────────────

        [Fact]
        public void Constructor_ArchivoNoExiste_LanzaFileNotFoundException()
        {
            var mockFileReaderThrows = new Mock<IJsonFileReader>();
            mockFileReaderThrows.Setup(f => f.JsonPath).Returns(_fakePath);
            mockFileReaderThrows.Setup(f => f.ReadAllText(It.IsAny<string>()))
                .Throws<FileNotFoundException>();

            Assert.Throws<FileNotFoundException>(() =>
                new ProductRepository(_mockLogger.Object, _mockEnv.Object, mockFileReaderThrows.Object));
        }

        [Fact]
        public void Constructor_JsonInvalido_LanzaJsonException()
        {
            var mockFileReaderThrows = new Mock<IJsonFileReader>();
            mockFileReaderThrows.Setup(f => f.JsonPath).Returns(_fakePath);
            mockFileReaderThrows.Setup(f => f.ReadAllText(It.IsAny<string>()))
                .Returns("{ json inválido [[[");

            Assert.Throws<JsonException>(() =>
                new ProductRepository(_mockLogger.Object, _mockEnv.Object, mockFileReaderThrows.Object));
        }

        [Fact]
        public async Task Constructor_CargaProductosEnElDiccionario_CantidadCorrecta()
        {
            var repository = CreateRepository();

            var result = await repository.GetAllAsync();

            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task Constructor_CargaProductosEnElDiccionario_IdsComoClaves()
        {
            var repository = CreateRepository();

            var p1 = await repository.GetByIdAsync(1);
            var p2 = await repository.GetByIdAsync(2);
            var p3 = await repository.GetByIdAsync(3);

            Assert.NotNull(p1);
            Assert.NotNull(p2);
            Assert.NotNull(p3);
        }
    }
}
