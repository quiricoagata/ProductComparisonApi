using Microsoft.AspNetCore.Mvc;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase, IProductController
    {
        private readonly IProductService _productService;
        private readonly IProductValidator _validator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            IProductValidator validator,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/products
        /// Devuelve el catálogo completo de productos.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(Response<List<Product>>), 200)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var products = await _productService.GetAllAsync();
                return Ok(Response<List<Product>>.Ok(products, $"{products.Count} productos encontrados."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos.");
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }

        /// <summary>
        /// GET /api/products/{id}
        /// Devuelve el detalle de un producto específico.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Response<Product>), 200)]
        [ProducesResponseType(typeof(Response<object>), 400)]
        [ProducesResponseType(typeof(Response<object>), 404)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var idError = _validator.ValidateProductId(id);
                if (idError is not null)
                    return BadRequest(Response<object>.Fail(idError));

                var product = await _productService.GetByIdAsync(id);

                if (product is null)
                {
                    _logger.LogWarning("Producto con ID {Id} no encontrado.", id);
                    return NotFound(Response<object>.Fail($"No existe un producto con ID {id}."));
                }

                return Ok(Response<Product>.Ok(product));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el producto {Id}.", id);
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }

        /// <summary>
        /// GET /api/products/compare?ids=1&ids=2&ids=3
        /// Devuelve los productos solicitados para comparar.
        /// </summary>
        [HttpGet("compare")]
        [ProducesResponseType(typeof(Response<List<Product>>), 200)]
        [ProducesResponseType(typeof(Response<object>), 400)]
        [ProducesResponseType(typeof(Response<object>), 404)]
        public async Task<IActionResult> Compare([FromQuery] ComparisonRequest request)
        {
            try
            {
                var validationError = _validator.ValidateComparisonRequest(request);
                if (validationError is not null)
                    return BadRequest(Response<object>.Fail(validationError));

                var products = await _productService.GetByIdsAsync(request.ProductIds);

                var notFoundIds = request.ProductIds
                    .Except(products.Select(p => p.Id))
                    .ToList();

                if (notFoundIds.Any())
                    return NotFound(Response<object>.Fail(
                        $"No se encontraron productos con los IDs: {string.Join(", ", notFoundIds)}."));

                _logger.LogInformation("Comparación solicitada para IDs: {Ids}",
                    string.Join(", ", request.ProductIds));

                return Ok(Response<List<Product>>.Ok(products, $"Comparando {products.Count} productos."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al comparar productos.");
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }

        //////////////////// Endpoints de administración (CRUD) ////////////////////
        
        /// <summary>
        /// POST /api/products
        /// Crea un nuevo producto en el inventario en memoria.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Response<Product>), 201)]
        [ProducesResponseType(typeof(Response<object>), 400)]
        public async Task<IActionResult> Create([FromBody] Product product)
        {
            try
            {
                var validationError = _validator.ValidateProduct(product);
                if (validationError is not null)
                    return BadRequest(Response<object>.Fail(validationError));

                var created = await _productService.CreateAsync(product);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = created.Id },
                    Response<Product>.Ok(created, "Producto creado correctamente."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto.");
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }

        /// <summary>
        /// PUT /api/products/{id}
        /// Reemplaza todos los campos de un producto existente.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(Response<Product>), 200)]
        [ProducesResponseType(typeof(Response<object>), 400)]
        [ProducesResponseType(typeof(Response<object>), 404)]
        public async Task<IActionResult> Update(int id, [FromBody] Product product)
        {
            try
            {
                var idError = _validator.ValidateProductId(id);
                if (idError is not null)
                    return BadRequest(Response<object>.Fail(idError));

                var bodyError = _validator.ValidateProduct(product);
                if (bodyError is not null)
                    return BadRequest(Response<object>.Fail(bodyError));

                var updated = await _productService.UpdateAsync(id, product);
                return Ok(Response<Product>.Ok(updated, "Producto actualizado correctamente."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Response<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el producto {Id}.", id);
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }

        /// <summary>
        /// PATCH /api/products/{id}
        /// Actualiza parcialmente un producto. Solo se modifican los campos enviados en el body.
        /// Los campos no incluidos conservan su valor original.
        /// </summary>
        [HttpPatch("{id:int}")]
        [ProducesResponseType(typeof(Response<Product>), 200)]
        [ProducesResponseType(typeof(Response<object>), 400)]
        [ProducesResponseType(typeof(Response<object>), 404)]
        public async Task<IActionResult> PartialUpdate(int id, [FromBody] UpdateProductRequest request)
        {
            try
            {
                var idError = _validator.ValidateProductId(id);
                if (idError is not null)
                    return BadRequest(Response<object>.Fail(idError));

                var bodyError = _validator.ValidatePartialProduct(request);
                if (bodyError is not null)
                    return BadRequest(Response<object>.Fail(bodyError));

                var updated = await _productService.PartialUpdateAsync(id, request);
                return Ok(Response<Product>.Ok(updated, "Producto actualizado parcialmente."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(Response<object>.Fail(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el producto {Id}.", id);
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }

        /// <summary>
        /// DELETE /api/products/{id}
        /// Elimina un producto del inventario en memoria.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(typeof(Response<object>), 200)]
        [ProducesResponseType(typeof(Response<object>), 400)]
        [ProducesResponseType(typeof(Response<object>), 404)]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var idError = _validator.ValidateProductId(id);
                if (idError is not null)
                    return BadRequest(Response<object>.Fail(idError));

                var deleted = await _productService.DeleteAsync(id);

                if (!deleted)
                    return NotFound(Response<object>.Fail($"No existe un producto con ID {id}."));

                return Ok(Response<object>.Empty($"Producto {id} eliminado correctamente."));
            }
            catch (Exception ex)
            {
                
                _logger.LogError(ex, "Error al eliminar el producto {Id}.", id);
                return StatusCode(500, Response<object>.Fail("Error interno del servidor."));
            }
        }
    }
}