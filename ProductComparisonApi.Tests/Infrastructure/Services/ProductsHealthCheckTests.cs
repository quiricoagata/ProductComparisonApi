using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Infrastructure.HealthChecks;
using ProductComparisonApi.Infrastructure.Services;

namespace ProductComparisonApi.Tests.Infrastructure.Services
{
    public class ProductsHealthCheckTests
    {
        private readonly Mock<IProductService> _mockService;
        private readonly ProductsHealthCheck _healthCheck;

        public ProductsHealthCheckTests()
        {
            _mockService = new Mock<IProductService>();
            _healthCheck = new ProductsHealthCheck(_mockService.Object);
        }

        [Fact]
        public async Task CheckHealthAsync_FuenteDeDatosAccesible_RetornaHealthy()
        {
            _mockService.Setup(s => s.IsHealthyAsync()).ReturnsAsync(true);

            var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Healthy, result.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_FuenteDeDatosNoDisponible_RetornaUnhealthy()
        {
            _mockService.Setup(s => s.IsHealthyAsync()).ReturnsAsync(false);

            var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }

        [Fact]
        public async Task CheckHealthAsync_ErrorInesperado_RetornaUnhealthy()
        {
            _mockService.Setup(s => s.IsHealthyAsync()).ThrowsAsync(new Exception("Error inesperado."));

            var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

            Assert.Equal(HealthStatus.Unhealthy, result.Status);
        }
    }
}