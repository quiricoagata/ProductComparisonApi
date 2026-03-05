using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ProductComparisonApi.Controllers;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IProductService> _mockService;
        private readonly Mock<IProductValidator> _mockValidator;
        private readonly Mock<ILogger<ProductsController>> _mockLogger;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
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
        public async Task GetAll_ExistenProductos_Retorna200ConListaYMensaje()
        {
            // Arrange
            var products = new List<Product>
            {
                new() { Id = 1, Nombre = "Laptop 1" },
                new() { Id = 2, Nombre = "Laptop 2" },
                new() { Id = 3, Nombre = "Laptop 3" }
            };

            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<List<Product>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(3, response.Data!.Count);
            Assert.Contains("3 productos encontrados", response.Message ?? string.Empty);
        }

        [Fact]
        public async Task GetAll_ListaVacia_Retorna200ConMensajeCero()
        {
            // Arrange
            var products = new List<Product>();
            _mockService.Setup(s => s.GetAllAsync()).ReturnsAsync(products);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<List<Product>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Empty(response.Data!);
            Assert.Contains("0 productos encontrados", response.Message ?? string.Empty);
        }

        [Fact]
        public async Task GetAll_ServiceThrows_Retorna500()
        {
            // Arrange
            _mockService.Setup(s => s.GetAllAsync()).ThrowsAsync(new Exception("Error inesperado"));

            // Act
            var result = await _controller.GetAll();

            // Assert
            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
            var response = Assert.IsType<Response<object>>(statusResult.Value);
            Assert.False(response.Success);
        }

        // ── Compare ───────────────────────────────────────────────────

        [Fact]
        public async Task Compare_RequestValido_Retorna200ConProductos()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { 1, 2 } };
            var products = new List<Product>
            {
                new() { Id = 1, Nombre = "Laptop 1" },
                new() { Id = 2, Nombre = "Laptop 2" }
            };

            _mockValidator.Setup(v => v.ValidateComparisonRequest(request)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdsAsync(request.ProductIds)).ReturnsAsync(products);

            var result = await _controller.Compare(request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<List<Product>>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(2, response.Data!.Count);
        }

        [Fact]
        public async Task Compare_RequestInvalido_Retorna400()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { } };

            _mockValidator.Setup(v => v.ValidateComparisonRequest(request)).Returns("Se deben enviar IDs para comparar.");

            var result = await _controller.Compare(request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Response<object>>(badRequest.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task Compare_AlgunosIdsNoEncontrados_Retorna404()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { 1, 99 } };
            var products = new List<Product> { new() { Id = 1, Nombre = "Laptop 1" } };

            _mockValidator.Setup(v => v.ValidateComparisonRequest(request)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdsAsync(request.ProductIds)).ReturnsAsync(products);

            var result = await _controller.Compare(request);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Compare_ServiceThrows_Retorna500()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { 1 } };

            _mockValidator.Setup(v => v.ValidateComparisonRequest(request)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdsAsync(request.ProductIds)).ThrowsAsync(new Exception("boom"));

            var result = await _controller.Compare(request);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            var response = Assert.IsType<Response<object>>(obj.Value);
            Assert.False(response.Success);
        }

        // ── PartialUpdate ─────────────────────────────────────────────

        [Fact]
        public async Task PartialUpdate_Valido_Retorna200()
        {
            var request = new UpdateProductRequest { Precio = 199.99m };
            var updated = new Product { Id = 1, Nombre = "Laptop", Precio = 199.99m };

            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidatePartialProduct(request)).Returns((string?)null);
            _mockService.Setup(s => s.PartialUpdateAsync(1, request)).ReturnsAsync(updated);

            var result = await _controller.PartialUpdate(1, request);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var response = Assert.IsType<Response<Product>>(okResult.Value);
            Assert.True(response.Success);
            Assert.Equal(199.99m, response.Data!.Precio);
        }

        [Fact]
        public async Task PartialUpdate_IdInvalido_Retorna400()
        {
            var request = new UpdateProductRequest { Precio = 199.99m };
            _mockValidator.Setup(v => v.ValidateProductId(-1)).Returns("El ID debe ser positivo.");

            var result = await _controller.PartialUpdate(-1, request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Response<object>>(badRequest.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task PartialUpdate_BodyInvalido_Retorna400()
        {
            var request = new UpdateProductRequest { Precio = -1m }; // ejemplo inválido
            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidatePartialProduct(request)).Returns("Precio inválido.");

            var result = await _controller.PartialUpdate(1, request);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Response<object>>(badRequest.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task PartialUpdate_IdNoExistente_Retorna404()
        {
            var request = new UpdateProductRequest { Precio = 199.99m };

            _mockValidator.Setup(v => v.ValidateProductId(99)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidatePartialProduct(request)).Returns((string?)null);
            _mockService.Setup(s => s.PartialUpdateAsync(99, request)).ThrowsAsync(new KeyNotFoundException("No existe."));

            var result = await _controller.PartialUpdate(99, request);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task PartialUpdate_ServiceThrows_Retorna500()
        {
            var request = new UpdateProductRequest { Precio = 9.99m };

            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidatePartialProduct(request)).Returns((string?)null);
            _mockService.Setup(s => s.PartialUpdateAsync(1, request)).ThrowsAsync(new Exception("boom"));

            var result = await _controller.PartialUpdate(1, request);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
            var response = Assert.IsType<Response<object>>(obj.Value);
            Assert.False(response.Success);
        }

        // ── Create (error servicio) ───────────────────────────────────

        [Fact]
        public async Task Create_ServiceThrowsException_Retorna500()
        {
            var product = new Product { Nombre = "Laptop", Precio = 100 };
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.CreateAsync(product)).ThrowsAsync(new Exception("DB error"));

            var result = await _controller.Create(product);

            var statusResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, statusResult.StatusCode);
        }

        // ── Update / Delete validación 400 ─────────────────────────────

        [Fact]
        public async Task Update_IdInvalido_Retorna400()
        {
            var product = new Product { Nombre = "Laptop", Precio = 100 };
            _mockValidator.Setup(v => v.ValidateProductId(-5)).Returns("El ID debe ser positivo.");

            var result = await _controller.Update(-5, product);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Response<object>>(badRequest.Value);
            Assert.False(response.Success);
        }

        [Fact]
        public async Task Delete_IdInvalido_Retorna400()
        {
            _mockValidator.Setup(v => v.ValidateProductId(-10)).Returns("El ID debe ser positivo.");

            var result = await _controller.Delete(-10);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            var response = Assert.IsType<Response<object>>(badRequest.Value);
            Assert.False(response.Success);
        }

        // GET /api/products/{id}
        [Fact]
        public async Task GetById_Valido_RetornaOkConProductoYMensaje()
        {
            var product = new Product { Id = 10, Nombre = "Test" };
            _mockValidator.Setup(v => v.ValidateProductId(10)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(product);

            var result = await _controller.GetById(10);

            var ok = Assert.IsType<OkObjectResult>(result);
            var resp = Assert.IsType<Response<Product>>(ok.Value);
            Assert.True(resp.Success);
            Assert.Equal(10, resp.Data!.Id);
            // Mensaje opcional: si no viene, al menos comprobar que existe Response
        }

        [Fact]
        public async Task GetById_IdInvalido_RetornaBadRequest()
        {
            _mockValidator.Setup(v => v.ValidateProductId(0)).Returns("ID inválido");

            var result = await _controller.GetById(0);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var resp = Assert.IsType<Response<object>>(bad.Value);
            Assert.False(resp.Success);
            Assert.Contains("ID inválido", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task GetById_NoEncontrado_RetornaNotFound()
        {
            _mockValidator.Setup(v => v.ValidateProductId(99)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((Product?)null);

            var result = await _controller.GetById(99);

            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            var resp = Assert.IsType<Response<object>>(notFound.Value);
            Assert.False(resp.Success);
            Assert.Contains("No existe un producto", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task GetById_ServicioLanzaExcepcion_Retorna500()
        {
            _mockValidator.Setup(v => v.ValidateProductId(5)).Returns((string?)null);
            _mockService.Setup(s => s.GetByIdAsync(5)).ThrowsAsync(new Exception("boom"));

            var result = await _controller.GetById(5);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // POST /api/products
        [Fact]
        public async Task Create_Valido_RetornaCreatedAtActionYContenido()
        {
            var product = new Product { Nombre = "Nuevo", Precio = 50m };
            var created = new Product { Id = 123, Nombre = "Nuevo", Precio = 50m };

            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.CreateAsync(product)).ReturnsAsync(created);

            var result = await _controller.Create(product);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(_controller.GetById), createdResult.ActionName);
            Assert.True(createdResult.RouteValues!.ContainsKey("id"));
            Assert.Equal(created.Id, createdResult.RouteValues["id"]);

            var resp = Assert.IsType<Response<Product>>(createdResult.Value);
            Assert.True(resp.Success);
            Assert.Equal(123, resp.Data!.Id);
            Assert.Contains("Producto creado", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task Create_ValidadorDevuelveError_Retorna400()
        {
            var product = new Product { Nombre = "" };
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns("Nombre obligatorio");

            var result = await _controller.Create(product);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var resp = Assert.IsType<Response<object>>(bad.Value);
            Assert.False(resp.Success);
            Assert.Contains("Nombre obligatorio", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task Create_ServicioLanzaException_Retorna500()
        {
            var product = new Product { Nombre = "X" };
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.CreateAsync(product)).ThrowsAsync(new Exception("DB fail"));

            var result = await _controller.Create(product);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // PUT /api/products/{id}
        [Fact]
        public async Task Update_Valido_RetornaOkConObjetoActualizado()
        {
            var product = new Product { Nombre = "P", Precio = 10m };
            var updated = new Product { Id = 7, Nombre = "P", Precio = 10m };

            _mockValidator.Setup(v => v.ValidateProductId(7)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.UpdateAsync(7, product)).ReturnsAsync(updated);

            var result = await _controller.Update(7, product);

            var ok = Assert.IsType<OkObjectResult>(result);
            var resp = Assert.IsType<Response<Product>>(ok.Value);
            Assert.True(resp.Success);
            Assert.Equal(7, resp.Data!.Id);
            Assert.Contains("actualizado", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task Update_ValidadorIdFallido_Retorna400()
        {
            var product = new Product { Nombre = "P" };
            _mockValidator.Setup(v => v.ValidateProductId(-2)).Returns("ID invalido");

            var result = await _controller.Update(-2, product);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var resp = Assert.IsType<Response<object>>(bad.Value);
            Assert.False(resp.Success);
        }

        [Fact]
        public async Task Update_ValidadorBodyFallido_Retorna400()
        {
            var product = new Product { Nombre = "" };
            _mockValidator.Setup(v => v.ValidateProductId(1)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns("Nombre requerido");

            var result = await _controller.Update(1, product);

            var bad = Assert.IsType<BadRequestObjectResult>(result);
            var resp = Assert.IsType<Response<object>>(bad.Value);
            Assert.False(resp.Success);
            Assert.Contains("Nombre requerido", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task Update_NoEncontrado_Retorna404()
        {
            var product = new Product { Nombre = "P" };
            _mockValidator.Setup(v => v.ValidateProductId(99)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.UpdateAsync(99, product)).ThrowsAsync(new KeyNotFoundException("no existe"));

            var result = await _controller.Update(99, product);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Update_ServicioLanzaException_Retorna500()
        {
            var product = new Product { Nombre = "Z" };
            _mockValidator.Setup(v => v.ValidateProductId(2)).Returns((string?)null);
            _mockValidator.Setup(v => v.ValidateProduct(product)).Returns((string?)null);
            _mockService.Setup(s => s.UpdateAsync(2, product)).ThrowsAsync(new Exception("err"));

            var result = await _controller.Update(2, product);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }

        // DELETE /api/products/{id}
        [Fact]
        public async Task Delete_Valido_RetornaOkConMensaje()
        {
            _mockValidator.Setup(v => v.ValidateProductId(3)).Returns((string?)null);
            _mockService.Setup(s => s.DeleteAsync(3)).ReturnsAsync(true);

            var result = await _controller.Delete(3);

            var ok = Assert.IsType<OkObjectResult>(result);
            var resp = Assert.IsType<Response<object>>(ok.Value);
            Assert.True(resp.Success);
            Assert.Contains("Producto 3 eliminado correctamente", resp.Message ?? string.Empty);
        }

        [Fact]
        public async Task Delete_NoEncontrado_Retorna404()
        {
            _mockValidator.Setup(v => v.ValidateProductId(50)).Returns((string?)null);
            _mockService.Setup(s => s.DeleteAsync(50)).ReturnsAsync(false);

            var result = await _controller.Delete(50);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task Delete_ServicioLanzaException_Retorna500()
        {
            _mockValidator.Setup(v => v.ValidateProductId(4)).Returns((string?)null);
            _mockService.Setup(s => s.DeleteAsync(4)).ThrowsAsync(new Exception("fail"));

            var result = await _controller.Delete(4);

            var obj = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, obj.StatusCode);
        }
    }
}