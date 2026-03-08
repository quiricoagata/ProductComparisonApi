using Microsoft.Extensions.Logging;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IProductRepository repository,
            ILogger<ProductService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<List<Product>> GetAllAsync()
        {
            try
            {
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los productos.");
                throw;
            }
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            try
            {
                return await _repository.GetByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el producto {Id}.", id);
                throw;
            }
        }

        public async Task<List<Product>> GetByIdsAsync(List<int> ids)
        {
            try
            {
                return await _repository.GetByIdsAsync(ids);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por IDs.");
                throw;
            }
        }

        public async Task<Product> CreateAsync(Product product)
        {
            try
            {
                return await _repository.CreateAsync(product);
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
                return await _repository.UpdateAsync(id, updated);
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
                return await _repository.PartialUpdateAsync(id, request);
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
                return await _repository.DeleteAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el producto {Id}.", id);
                throw;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                return await _repository.IsHealthyAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar la salud del repositorio.");
                throw;
            }
        }
    }
}