using ProductComparisonApi.Domain.Models;

namespace ProductComparisonApi.API.Validator
{

    public class ProductValidator 
    {

        public string? ValidateComparisonRequest(ComparisonRequest request)
        {
            if (request.ProductIds == null || request.ProductIds.Count < 2)
                return "Debes enviar al menos 2 IDs de productos.";

            if (request.ProductIds.Distinct().Count() != request.ProductIds.Count)
                return "La lista contiene IDs duplicados.";

            return null;
        }

        public string? ValidateProduct(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Nombre))
                return "El nombre del producto es obligatorio.";

            if (string.IsNullOrWhiteSpace(product.Descripcion))
                return "La descripción del producto es obligatoria.";

            if (string.IsNullOrWhiteSpace(product.UrlImagen))
                return "La URL de imagen es obligatoria.";

            if (product.Precio <= 0)
                return "El precio debe ser mayor a cero.";

            if (product.Calificacion < 0 || product.Calificacion > 5)
                return "La calificación debe estar entre 0 y 5.";

            return null;
        }

        /// <summary>
        /// Valida los campos enviados en un PATCH.
        /// Solo valida los campos que vienen en el body, ignora los null.
        /// </summary>
        public string? ValidatePartialProduct(UpdateProductRequest request)
        {
            if (request.Nombre is not null && string.IsNullOrWhiteSpace(request.Nombre))
                return "El nombre no puede ser vacío.";

            if (request.Descripcion is not null && string.IsNullOrWhiteSpace(request.Descripcion))
                return "La descripción no puede ser vacía.";

            if (request.UrlImagen is not null && string.IsNullOrWhiteSpace(request.UrlImagen))
                return "La URL de imagen no puede ser vacía.";

            if (request.Precio is not null && request.Precio <= 0)
                return "El precio debe ser mayor a cero.";

            if (request.Calificacion is not null && (request.Calificacion < 0 || request.Calificacion > 5))
                return "La calificación debe estar entre 0 y 5.";

            return null;
        }

        public string? ValidateProductId(int id)
        {
            if (id <= 0)
                return "El ID debe ser un número positivo.";

            return null;
        }
    }
}