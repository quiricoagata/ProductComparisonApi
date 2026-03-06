using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductComparisonApi.Domain.Interfaces;

namespace ProductComparisonApi.Infrastructure.HealthChecks
{

    public class ProductsHealthCheck : IHealthCheck
    {
        private readonly IProductService _productService;

        public ProductsHealthCheck(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Verifica que la fuente de datos esté disponible y sea accesible.
        /// </summary>
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var isHealthy = await _productService.IsHealthyAsync();

                return isHealthy
                    ? HealthCheckResult.Healthy("La fuente de datos es accesible.")
                    : HealthCheckResult.Unhealthy("La fuente de datos no está disponible.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy($"Error inesperado: {ex.Message}");
            }
        }
    }
}