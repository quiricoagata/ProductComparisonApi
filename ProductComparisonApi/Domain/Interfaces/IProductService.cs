using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Domain.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> GetAllAsync();
        Task<Product?> GetByIdAsync(int id);
        Task<List<Product>> GetByIdsAsync(List<int> ids);
        Task<Product> CreateAsync(Product product);  
        Task<Product> UpdateAsync(int id, Product updated);

        Task<Product> PartialUpdateAsync(int id, UpdateProductRequest updated);
        Task<bool> DeleteAsync(int id);
    }
}