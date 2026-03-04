namespace ProductComparisonApi.Domain.Models
{

    public class UpdateProductRequest
    {
        public string? Nombre { get; set; }
        public string? UrlImagen { get; set; }
        public string? Descripcion { get; set; }
        public decimal? Precio { get; set; }
        public double? Calificacion { get; set; }
        public Dictionary<string, string>? Especificaciones { get; set; }
    }
}