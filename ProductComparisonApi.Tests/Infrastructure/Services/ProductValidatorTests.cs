using ProductComparisonApi.Domain.Models;
using ProductComparisonApi.Infrastructure.Services;

namespace ProductComparisonApi.Tests.Infrastructure.Services
{
    public class ProductValidatorTests
    {
        private readonly ProductValidator _validator;

        public ProductValidatorTests()
        {
            // No necesita mocks — no tiene dependencias externas
            _validator = new ProductValidator();
        }

        // ── ValidateProductId ─────────────────────────────────────────

        [Fact]
        public void ValidateProductId_IdValido_RetornaNull()
        {
            var result = _validator.ValidateProductId(1);
            Assert.Null(result);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        public void ValidateProductId_IdInvalido_RetornaMensajeError(int id)
        {
            var result = _validator.ValidateProductId(id);
            Assert.NotNull(result);
        }

        // ── ValidateProduct ───────────────────────────────────────────

        [Fact]
        public void ValidateProduct_ProductoValido_RetornaNull()
        {
            var product = new Product
            {
                Nombre = "Laptop",
                Descripcion = "Descripcion",
                UrlImagen = "https://imagen.com",
                Precio = 999,
                Calificacion = 4.5
            };

            var result = _validator.ValidateProduct(product);
            Assert.Null(result);
        }

        [Theory]
        [InlineData("", "Descripcion", "https://imagen.com", 999, 4.5)]
        [InlineData("Laptop", "", "https://imagen.com", 999, 4.5)]
        [InlineData("Laptop", "Descripcion", "", 999, 4.5)]
        [InlineData("Laptop", "Descripcion", "https://imagen.com", 0, 4.5)]
        [InlineData("Laptop", "Descripcion", "https://imagen.com", -1, 4.5)]
        [InlineData("Laptop", "Descripcion", "https://imagen.com", 999, -1)]
        [InlineData("Laptop", "Descripcion", "https://imagen.com", 999, 6)]
        public void ValidateProduct_CamposInvalidos_RetornaMensajeError(
            string nombre, string descripcion, string urlImagen, decimal precio, double calificacion)
        {
            var product = new Product
            {
                Nombre = nombre,
                Descripcion = descripcion,
                UrlImagen = urlImagen,
                Precio = precio,
                Calificacion = calificacion
            };

            var result = _validator.ValidateProduct(product);
            Assert.NotNull(result);
        }

        // ── ValidateComparisonRequest ─────────────────────────────────

        [Fact]
        public void ValidateComparisonRequest_IdsValidos_RetornaNull()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { 1, 2, 3 } };
            var result = _validator.ValidateComparisonRequest(request);
            Assert.Null(result);
        }

        [Fact]
        public void ValidateComparisonRequest_UnSoloId_RetornaMensajeError()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { 1 } };
            var result = _validator.ValidateComparisonRequest(request);
            Assert.NotNull(result);
        }

        [Fact]
        public void ValidateComparisonRequest_IdsDuplicados_RetornaMensajeError()
        {
            var request = new ComparisonRequest { ProductIds = new List<int> { 1, 1 } };
            var result = _validator.ValidateComparisonRequest(request);
            Assert.NotNull(result);
        }

        [Fact]
        public void ValidateComparisonRequest_ListaVacia_RetornaMensajeError()
        {
            var request = new ComparisonRequest { ProductIds = new List<int>() };
            var result = _validator.ValidateComparisonRequest(request);
            Assert.NotNull(result);
        }

        // ── ValidatePartialProduct ────────────────────────────────────

        [Fact]
        public void ValidatePartialProduct_TodosNulos_RetornaNull()
        {
            // Si no se envía ningún campo es válido — el controlador decide si tiene sentido
            var request = new UpdateProductRequest();
            var result = _validator.ValidatePartialProduct(request);
            Assert.Null(result);
        }

        [Fact]
        public void ValidatePartialProduct_NombreVacio_RetornaMensajeError()
        {
            var request = new UpdateProductRequest { Nombre = "" };
            var result = _validator.ValidatePartialProduct(request);
            Assert.NotNull(result);
        }

        [Fact]
        public void ValidatePartialProduct_PrecioNegativo_RetornaMensajeError()
        {
            var request = new UpdateProductRequest { Precio = -1 };
            var result = _validator.ValidatePartialProduct(request);
            Assert.NotNull(result);
        }

        [Fact]
        public void ValidatePartialProduct_CalificacionFueraDeRango_RetornaMensajeError()
        {
            var request = new UpdateProductRequest { Calificacion = 6 };
            var result = _validator.ValidatePartialProduct(request);
            Assert.NotNull(result);
        }
    }
}