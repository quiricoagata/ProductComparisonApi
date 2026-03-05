using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly ConcurrentDictionary<int, Product> _products = new();
        private readonly IJsonFileReader _fileReader;
        private readonly ILogger<ProductService> _logger;
        private readonly string _jsonPath;

        // Permite solo una escritura al JSON a la vez
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ProductService(
            ILogger<ProductService> logger,
            IWebHostEnvironment env,
            IJsonFileReader fileReader)
        {
            _logger = logger;
            _fileReader = fileReader;
            _jsonPath = Path.Combine(env.ContentRootPath, "Data", "products.json");

            if (!_fileReader.FileExists(_jsonPath))
            {
                _logger.LogError("Archivo de datos no encontrado: {Path}", _jsonPath);
                throw new FileNotFoundException("No se encontró el archivo de productos.", _jsonPath);
            }

            var jsonContent = _fileReader.ReadAllText(_jsonPath);
            var products = JsonSerializer.Deserialize<List<Product>>(jsonContent, _jsonOptions)
                ?? new List<Product>();

            // Carga en el diccionario usando el ID como clave
            foreach (var product in products)
                _products[product.Id] = product;

            _logger.LogInformation("ProductService inicializado con {Count} productos.", _products.Count);
        }

        public Task<List<Product>> GetAllAsync() =>
            Task.FromResult(_products.Values.ToList());

        public Task<Product?> GetByIdAsync(int id) =>
            Task.FromResult(_products.TryGetValue(id, out var product) ? product : null);

   
        public Task<List<Product>> GetByIdsAsync(List<int> ids) =>
            Task.FromResult(ids
                .Where(id => _products.ContainsKey(id))
                .Select(id => _products[id])
                .ToList());

        public async Task<Product> CreateAsync(Product product)
        {
            product.Id = _products.IsEmpty ? 1 : _products.Keys.Max() + 1;
            _products[product.Id] = product;
            await SaveChangesAsync();
            _logger.LogInformation("Producto creado con ID {Id}.", product.Id);
            return product;
        }

        public async Task<Product> UpdateAsync(int id, Product updated)
        {
            if (!_products.ContainsKey(id))
                throw new KeyNotFoundException($"No existe un producto con ID {id}.");

            updated.Id = id;
            _products[id] = updated;
            await SaveChangesAsync();
            _logger.LogInformation("Producto {Id} actualizado.", id);
            return updated;
        }

        public async Task<Product> PartialUpdateAsync(int id, UpdateProductRequest request)
        {
            if (!_products.TryGetValue(id, out var existing))
                throw new KeyNotFoundException($"No existe un producto con ID {id}.");

            if (request.Nombre is not null) existing.Nombre = request.Nombre;
            if (request.UrlImagen is not null) existing.UrlImagen = request.UrlImagen;
            if (request.Descripcion is not null) existing.Descripcion = request.Descripcion;
            if (request.Precio is not null) existing.Precio = request.Precio.Value;
            if (request.Calificacion is not null) existing.Calificacion = request.Calificacion.Value;
            if (request.Especificaciones is not null) existing.Especificaciones = request.Especificaciones;

            _products[id] = existing;
            await SaveChangesAsync();
            _logger.LogInformation("Producto {Id} actualizado parcialmente.", id);
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var removed = _products.TryRemove(id, out _);
            if (removed)
            {
                await SaveChangesAsync();
                _logger.LogInformation("Producto {Id} eliminado.", id);
            }
            return removed;
        }


        /// <summary>
        /// Verifica que la fuente de datos (archivo JSON) esté disponible y sea accesible.
        /// </summary>
        public Task<bool> IsHealthyAsync()
        {
            try
            {
                if (!_fileReader.FileExists(_jsonPath))
                    return Task.FromResult(false);

                var content = _fileReader.ReadAllText(_jsonPath);
                JsonSerializer.Deserialize<List<object>>(content);
                return Task.FromResult(true);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        /// <summary>
        /// Serializa el diccionario y escribe al JSON.
        /// El SemaphoreSlim garantiza que solo un request escriba a la vez.
        /// </summary>
        private async Task SaveChangesAsync()
        {
            await _writeLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(_products.Values.ToList(), _jsonOptions);
                await _fileReader.WriteAllTextAsync(_jsonPath, json);
                _logger.LogInformation("Cambios persistidos en {Path}.", _jsonPath);
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}