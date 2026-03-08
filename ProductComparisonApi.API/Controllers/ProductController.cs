using Microsoft.AspNetCore.Mvc;
using ProductComparisonApi.API.Validator;
using ProductComparisonApi.Application.Interfaces;
using ProductComparisonApi.Domain.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ProductComparisonApi.Controllers
{
    /// <summary>
    /// Controlador principal para la gestión y comparación de productos.
    /// Expone endpoints de consulta, comparación y administración del catálogo.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ProductValidator _validator;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ProductValidator validator,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _validator = validator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los productos disponibles en el catálogo.
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta:
        ///
        ///     GET /api/products
        ///     {
        ///         "success": true,
        ///         "message": "3 productos encontrados.",
        ///         "data": [
        ///             {
        ///                 "id": 1,
        ///                 "nombre": "Laptop Pro X1",
        ///                 "urlImagen": "https://img1.com",
        ///                 "descripcion": "Laptop de alto rendimiento",
        ///                 "precio": 1299.99,
        ///                 "calificacion": 4.7,
        ///                 "especificaciones": { "RAM": "16GB", "Almacenamiento": "512GB SSD" }
        ///             }
        ///         ]
        ///     }
        ///
        /// </remarks>
        /// <returns>Lista completa de productos ordenada por ID.</returns>
        /// <response code="200">Lista de productos obtenida correctamente.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Obtiene todos los productos",
            Description = "Devuelve el catálogo completo de productos ordenado por ID.",
            Tags = new[] { "1. Consulta" })]
        [ProducesResponseType(typeof(Response<List<Product>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
        /// Obtiene el detalle de un producto específico por su ID.
        /// </summary>
        /// <remarks>
        /// Ejemplo de respuesta:
        ///
        ///     GET /api/products/1
        ///     {
        ///         "success": true,
        ///         "message": null,
        ///         "data": {
        ///             "id": 1,
        ///             "nombre": "Laptop Pro X1",
        ///             "urlImagen": "https://img1.com",
        ///             "descripcion": "Laptop de alto rendimiento",
        ///             "precio": 1299.99,
        ///             "calificacion": 4.7,
        ///             "especificaciones": { "RAM": "16GB" }
        ///         }
        ///     }
        ///
        /// </remarks>
        /// <param name="id">ID numérico del producto. Debe ser un entero mayor a 0.</param>
        /// <returns>Producto correspondiente al ID proporcionado.</returns>
        /// <response code="200">Producto encontrado correctamente.</response>
        /// <response code="400">El ID proporcionado no es válido (debe ser mayor a 0).</response>
        /// <response code="404">No existe un producto con el ID proporcionado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obtiene un producto por ID",
            Description = "Devuelve el detalle completo de un producto a partir de su ID.",
            Tags = new[] { "1. Consulta" })]
        [ProducesResponseType(typeof(Response<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
        /// Compara múltiples productos lado a lado a partir de sus IDs.
        /// </summary>
        /// <remarks>
        /// Se deben enviar al menos 2 IDs distintos como query parameters.
        ///
        /// Ejemplo de request:
        ///
        ///     GET /api/products/compare?ids=1&amp;ids=2&amp;ids=3
        ///
        /// Ejemplo de respuesta:
        ///
        ///     {
        ///         "success": true,
        ///         "message": "Comparando 2 productos.",
        ///         "data": [
        ///             { "id": 1, "nombre": "Laptop Pro X1", "precio": 1299.99, "calificacion": 4.7 },
        ///             { "id": 2, "nombre": "Laptop UltraSlim", "precio": 999.99, "calificacion": 4.3 }
        ///         ]
        ///     }
        ///
        /// </remarks>
        /// <param name="request">Lista de IDs a comparar. Mínimo 2 IDs, sin valores duplicados.</param>
        /// <returns>Lista de productos correspondientes a los IDs solicitados.</returns>
        /// <response code="200">Productos encontrados y retornados correctamente.</response>
        /// <response code="400">La lista de IDs no es válida (menos de 2 IDs o contiene duplicados).</response>
        /// <response code="404">Uno o más IDs no existen en el catálogo.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpGet("compare")]
        [SwaggerOperation(
            Summary = "Compara múltiples productos",
            Description = "Devuelve los productos solicitados para comparar lado a lado. Requiere mínimo 2 IDs distintos.",
            Tags = new[] { "1. Consulta" })]
        [ProducesResponseType(typeof(Response<List<Product>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
        /// Crea un nuevo producto en el catálogo. El ID se genera automáticamente.
        /// </summary>
        /// <remarks>
        /// Ejemplo de request:
        ///
        ///     POST /api/products
        ///     {
        ///         "nombre": "Laptop Nueva",
        ///         "urlImagen": "https://img.com/nueva.jpg",
        ///         "descripcion": "Laptop de última generación",
        ///         "precio": 799.99,
        ///         "calificacion": 4.2,
        ///         "especificaciones": {
        ///             "RAM": "8GB",
        ///             "Almacenamiento": "256GB SSD"
        ///         }
        ///     }
        ///
        /// </remarks>
        /// <param name="product">
        /// Datos del producto a crear. El campo ID es ignorado — se asigna automáticamente.
        /// Campos requeridos: Nombre, UrlImagen, Descripcion, Precio (mayor a 0), Calificacion (0 a 5).
        /// </param>
        /// <returns>Producto creado con su ID asignado y la URL de acceso en el header Location.</returns>
        /// <response code="201">Producto creado correctamente.</response>
        /// <response code="400">Los datos del producto no son válidos.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Crea un nuevo producto",
            Description = "Agrega un nuevo producto al catálogo. El ID se genera automáticamente.",
            Tags = new[] { "2. Administración" })]
        [ProducesResponseType(typeof(Response<Product>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
        /// Reemplaza todos los campos de un producto existente.
        /// </summary>
        /// <remarks>
        /// Todos los campos son requeridos. Para actualizar solo algunos campos usá PATCH /api/products/{id}.
        ///
        /// Ejemplo de request:
        ///
        ///     PUT /api/products/1
        ///     {
        ///         "nombre": "Laptop Pro X1 Actualizada",
        ///         "urlImagen": "https://img.com/x1-v2.jpg",
        ///         "descripcion": "Versión actualizada con más RAM",
        ///         "precio": 1399.99,
        ///         "calificacion": 4.8,
        ///         "especificaciones": {
        ///             "RAM": "32GB",
        ///             "Almacenamiento": "1TB SSD"
        ///         }
        ///     }
        ///
        /// </remarks>
        /// <param name="id">ID del producto a actualizar. Debe ser mayor a 0.</param>
        /// <param name="product">Datos completos del producto. Todos los campos son requeridos.</param>
        /// <returns>Producto con todos los campos actualizados.</returns>
        /// <response code="200">Producto actualizado correctamente.</response>
        /// <response code="400">El ID o los datos del producto no son válidos.</response>
        /// <response code="404">No existe un producto con el ID proporcionado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Actualiza un producto completo",
            Description = "Reemplaza todos los campos de un producto existente. Todos los campos son requeridos.",
            Tags = new[] { "2. Administración" })]
        [ProducesResponseType(typeof(Response<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
        /// Actualiza parcialmente un producto existente.
        /// Solo se modifican los campos enviados en el body — los demás conservan su valor original.
        /// </summary>
        /// <remarks>
        /// A diferencia de PUT, solo es necesario enviar los campos que se quieren modificar.
        ///
        /// Ejemplo para actualizar solo el precio:
        ///
        ///     PATCH /api/products/1
        ///     {
        ///         "precio": 1199.99
        ///     }
        ///
        /// </remarks>
        /// <param name="id">ID del producto a actualizar parcialmente. Debe ser mayor a 0.</param>
        /// <param name="request">Campos a modificar. Los campos con valor null se ignoran.</param>
        /// <returns>Producto con los campos enviados actualizados y el resto sin cambios.</returns>
        /// <response code="200">Producto actualizado parcialmente de forma correcta.</response>
        /// <response code="400">El ID o los campos enviados no son válidos.</response>
        /// <response code="404">No existe un producto con el ID proporcionado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpPatch("{id:int}")]
        [SwaggerOperation(
            Summary = "Actualiza parcialmente un producto",
            Description = "Modifica solo los campos enviados en el body. Los campos no incluidos conservan su valor original.",
            Tags = new[] { "2. Administración" })]
        [ProducesResponseType(typeof(Response<Product>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
        /// Elimina un producto del catálogo por su ID.
        /// </summary>
        /// <remarks>
        /// La eliminación es permanente — el cambio se persiste en el archivo JSON.
        ///
        /// Ejemplo de respuesta:
        ///
        ///     DELETE /api/products/1
        ///     {
        ///         "success": true,
        ///         "message": "Producto 1 eliminado correctamente.",
        ///         "data": null
        ///     }
        ///
        /// </remarks>
        /// <param name="id">ID del producto a eliminar. Debe ser mayor a 0.</param>
        /// <returns>Confirmación de la eliminación.</returns>
        /// <response code="200">Producto eliminado correctamente.</response>
        /// <response code="400">El ID proporcionado no es válido (debe ser mayor a 0).</response>
        /// <response code="404">No existe un producto con el ID proporcionado.</response>
        /// <response code="500">Error interno del servidor.</response>
        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Elimina un producto",
            Description = "Elimina permanentemente un producto del catálogo. El cambio se persiste en el archivo JSON.",
            Tags = new[] { "2. Administración" })]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Response<object>), StatusCodes.Status500InternalServerError)]
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
