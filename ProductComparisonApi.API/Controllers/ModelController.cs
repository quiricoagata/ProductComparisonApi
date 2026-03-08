using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using ProductComparisonApi.API.Validator;
using ProductComparisonApi.Application.Interfaces;

namespace ProductComparisonApi.Controllers
{
    [ExcludeFromCodeCoverage]
    [ApiExplorerSettings(IgnoreApi = true)]
    [Route("model")]
    public class ModelController : ProductsController
    {
        public ModelController(
            IProductService productService,
            ProductValidator validator,
            ILogger<ProductsController> logger)
            : base(productService, validator, logger)
        {
        }
    }
}