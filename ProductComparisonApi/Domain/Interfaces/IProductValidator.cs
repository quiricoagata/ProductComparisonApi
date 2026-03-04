using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Domain.Interfaces
{

    public interface IProductValidator
    {
        string? ValidateProductId(int id);
        string? ValidateProduct(Product product);
        string? ValidateComparisonRequest(ComparisonRequest request);
        string? ValidatePartialProduct(UpdateProductRequest request);
    }
}