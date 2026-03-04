using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductComparisonApi.Controllers;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Tests.Controllers
{
    public class ProductControllerTests
    {
        private readonly Mock<IProductService> _mockService;
        private readonly Mock<IProductValidator> _mockValidator;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly ProductsController _controller;

        public ProductControllerTests()
        {
            _mockService = new Mock<IProductService>();
            _mockValidator = new Mock<IProductValidator>();
            _mockLogger = new Mock<ILogger<ProductsController>>();

            _controller = new ProductsController(
                _mockService.Object,
                _mockValidator.Object,
                _mockLogger.Object);
        }

        // ── GetAll ────────────────────────────────────────────────────

        [Fact]
        public async Task GetAll_ExistenProductos_Retorna200ConLista()
        {
            // Arrange — preparamos los datos y configuramos los mocks
            var products = new List<Product>
            {
                new() { Id = 1, Nombre = "Laptop 1" },
                new() { Id = 2, Nombre = "Laptop 2" }
            };

            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

            // Act — ejecutamos el método
            var result = await _controller.GetAll();

            // Assert — verificamos el resultado
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<List<Product>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Count);
        }

        [Fact]
        public async Task GetAll_ErrorInesperado_Retorna500()
        {
            _mockService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("Error inesperado"));

            var result = await _controller.GetAll();

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        // ── GetById ───────────────────────────────────────────────────

        [Fact]
        public async Task GetById_IdExistente_Retorna200ConProducto()
        {
            var product = new Product { Id = 1, Nombre = "Laptop 1" };

            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);

            var result = await _controller.GetById(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<Product>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(1, response.Data!.Id);
        }

        [Fact]
        public async Task GetById_IdInvalido_Retorna400()
        {
            _mockValidator.Setup(v => v.ValidateProductId(-1)).Returns("El ID debe ser un número positivo.");

            var result = await _controller.GetById(-1);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Response<object>>(badRequest.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task GetById_IdNoExistente_Retorna404()
        {
            _mockValidator.Setup(v => v.ValidateProductId(99)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            var result = await _controller.GetById(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── Create ────────────────────────────────────────────────────

        [Fact]
        public async Task Create_ProductoValido_Retorna201()
        {
            var product = new Product { Nombre = "Laptop", Precio = 999, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };
            var created = new Product { Id = 1, Nombre = "Laptop", Precio = 999 };

            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.CreateAsync(product)).ReturnsAsync(created);

            var result = await _controller.Create(product);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task Create_ProductoInvalido_Retorna400()
        {
            var product = new Product { Nombre = "" };

            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns("El nombre es obligatorio.");

            var result = await _controller.Create(product);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        // ── Update ────────────────────────────────────────────────────

        [Fact]
        public async Task Update_ProductoExistente_Retorna200()
        {
            var product = new Product { Nombre = "Laptop", Precio = 999, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.UpdateAsync(1, product)).ReturnsAsync(product);

            var result = await _controller.Update(1, product);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<Product>>(okResult.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task Update_IdNoExistente_Retorna404()
        {
            var product = new Product { Nombre = "Laptop", Precio = 999, Calificacion = 4.5, Descripcion = "Desc", UrlImagen = "https://img.com" };

            _mockValidator.Setup(v => v.ValidateProductId(99)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.UpdateAsync(99, product)).ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Update(99, product);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── Delete ────────────────────────────────────────────────────

        [Fact]
        public async Task Delete_IdExistente_Retorna200()
        {
            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockService.Setup(s => s.DeleteAsync(1)).ReturnsAsync(true);

            var result = await _controller.Delete(1);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<object>>(okResult.Value);
            Assert.True(response.Success);
        }

        [Fact]
        public async Task Delete_IdNoExistente_Retorna404()
        {
            _mockValidator.Setup(v => v.ValidateProductId(99)).Returns((string?)null);
            _mockService.Setup(s => s.DeleteAsync(99)).ReturnsAsync(false);

            var result = await _controller.Delete(99);

            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
