using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly ConcurrentDictionary<int, Product> _products = new();
        private readonly IJsonFileReader _fileReader;
        private readonly ILogger<ProductRepository> _logger;
        private readonly string _jsonPath;
        private readonly SemaphoreSlim _writeLock = new(1, 1);

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        public ProductRepository(
            ILogger<ProductRepository> logger,
            IWebHostEnvironment env,
            IJsonFileReader fileReader)
        {
            _logger = logger;
            _fileReader = fileReader;
            _jsonPath = fileReader.JsonPath;

            _logger.LogInformation("Ruta del archivo de datos: {Path}", _jsonPath);

            try
            {
                var jsonContent = _fileReader.ReadAllText(_jsonPath);
                var products = JsonSerializer.Deserialize<List<Product>>(jsonContent, _jsonOptions)
                    ?? new List<Product>();

                foreach (var product in products)
                    _products[product.Id] = product;

                _logger.LogInformation("ProductRepository inicializado con {Count} productos.", _products.Count);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogError(ex, "Archivo de datos no encontrado: {Path}", _jsonPath);
                throw;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "El archivo de productos contiene JSON inválido.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar los productos.");
                throw;
            }
        }

        public Task<List<Product>> GetAllAsync() =>
            Task.FromResult(_products.Values.OrderBy(p => p.Id).ToList());

        public Task<Product?> GetByIdAsync(int id) =>
            Task.FromResult(_products.TryGetValue(id, out var product) ? product : null);

        public Task<List<Product>> GetByIdsAsync(List<int> ids) =>
            Task.FromResult(ids
                .Where(id => _products.ContainsKey(id))
                .Select(id => _products[id])
                .ToList());

        public async Task<Product> CreateAsync(Product product)
        {
            try
            {
                product.Id = _products.IsEmpty ? 1 : _products.Keys.Max() + 1;
                _products[product.Id] = product;
                await SaveChangesAsync();
                _logger.LogInformation("Producto creado con ID {Id}.", product.Id);
                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el producto.");
                throw;
            }
        }

        public async Task<Product> UpdateAsync(int id, Product updated)
        {
            try
            {
                if (!_products.ContainsKey(id))
                    throw new KeyNotFoundException($"No existe un producto con ID {id}.");

                updated.Id = id;
                _products[id] = updated;
                await SaveChangesAsync();
                _logger.LogInformation("Producto {Id} actualizado.", id);
                return updated;
            }
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el producto {Id}.", id);
                throw;
            }
        }

        public async Task<Product> PartialUpdateAsync(int id, UpdateProductRequest request)
        {
            try
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
            catch (KeyNotFoundException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar parcialmente el producto {Id}.", id);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            try
            {
                var removed = _products.TryRemove(id, out _);
                if (removed)
                {
                    await SaveChangesAsync();
                    _logger.LogInformation("Producto {Id} eliminado.", id);
                }
                return removed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto {Id}.", id);
                throw;
            }
        }

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

        private async Task SaveChangesAsync()
        {
            await _writeLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(_products.Values.ToList(), _jsonOptions);
                await _fileReader.WriteAllTextAsync(_jsonPath, json);
                _logger.LogInformation("Cambios persistidos en {Path}.", _jsonPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al persistir los cambios en {Path}.", _jsonPath);
                throw;
            }
            finally
            {
                _writeLock.Release();
            }
        }
    }
}