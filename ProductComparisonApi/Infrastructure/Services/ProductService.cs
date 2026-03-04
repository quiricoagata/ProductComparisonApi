using Microsoft.Extensions.Hosting;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;
using System.Text.Json;

namespace ProductComparisonApi.Infrastructure.Services
{
    public class ProductService : IProductService
    {
        private readonly List<Product> _products;
        private readonly IJsonFileReader _fileReader;
        private readonly ILogger<ProductService> _logger;
        private readonly string _jsonPath;

        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true  
        };

        public ProductService(ILogger<ProductService> logger, IWebHostEnvironment env, IJsonFileReader fileReader)
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
            _products = JsonSerializer.Deserialize<List<Product>>(jsonContent, _jsonOptions)
                ?? new List<Product>();

            _logger.LogInformation("ProductService inicializado con {Count} productos.", _products.Count);
        }

        public Task<List<Product>> GetAllAsync() =>
            Task.FromResult(_products);

        public Task<Product?> GetByIdAsync(int id) =>
            Task.FromResult(_products.FirstOrDefault(p => p.Id == id));

        public Task<List<Product>> GetByIdsAsync(List<int> ids) =>
            Task.FromResult(_products.Where(p => ids.Contains(p.Id)).ToList());

        public async Task<Product> CreateAsync(Product product)
        {
            product.Id = _products.Count > 0
                ? _products.Max(p => p.Id) + 1
                : 1;

            _products.Add(product);

            await SaveChangesAsync();

            _logger.LogInformation("Producto creado con ID {Id}.", product.Id);
            return product;
        }

        public async Task<Product> UpdateAsync(int id, Product updated)
        {
            var index = _products.FindIndex(p => p.Id == id);

            if (index == -1)
                throw new KeyNotFoundException($"No existe un producto con ID {id}.");

            updated.Id = id;
            _products[index] = updated;

            await SaveChangesAsync();

            _logger.LogInformation("Producto {Id} actualizado.", id);
            return _products[index];
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);

            if (product is null)
                return false;

            _products.Remove(product);

            await SaveChangesAsync();

            _logger.LogInformation("Producto {Id} eliminado.", id);
            return true;
        }

        /// <summary>
        /// Actualiza solo los campos que vienen en el request.
        /// Los campos null se ignoran y conservan su valor original.
        /// </summary>
        public async Task<Product> PartialUpdateAsync(int id, UpdateProductRequest request)
        {
            var index = _products.FindIndex(p => p.Id == id);

            if (index == -1)
                throw new KeyNotFoundException($"No existe un producto con ID {id}.");

            var existing = _products[index];

            if (request.Nombre is not null) existing.Nombre = request.Nombre;
            if (request.UrlImagen is not null) existing.UrlImagen = request.UrlImagen;
            if (request.Descripcion is not null) existing.Descripcion = request.Descripcion;
            if (request.Precio is not null) existing.Precio = request.Precio.Value;
            if (request.Calificacion is not null) existing.Calificacion = request.Calificacion.Value;
            if (request.Especificaciones is not null) existing.Especificaciones = request.Especificaciones;

            await SaveChangesAsync();

            _logger.LogInformation("Producto {Id} actualizado parcialmente.", id);
            return existing;
        }


        private async Task SaveChangesAsync()
        {
            var json = JsonSerializer.Serialize(_products, _jsonOptions);
            await _fileReader.WriteAllTextAsync(_jsonPath, json);
            _logger.LogInformation("Cambios persistidos en {Path}.", _jsonPath);
        }
    }
}
