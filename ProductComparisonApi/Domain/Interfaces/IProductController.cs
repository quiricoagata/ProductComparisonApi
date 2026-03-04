using Microsoft.AspNetCore.Mvc;
using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.Domain.Interfaces
{
    public interface IProductController
    {
        Task<IActionResult> GetAll();
        Task<IActionResult> GetById(int id);
        Task<IActionResult> Compare(ComparisonRequest request);  
        Task<IActionResult> Create(Product product); 
        Task<IActionResult> Update(int id, Product product);
        Task<IActionResult> PartialUpdate(int id, UpdateProductRequest request); 

        Task<IActionResult> Delete(int id);
    }
}