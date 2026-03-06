namespace ProductComparisonApi.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string UrlImagen { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public decimal Precio { get; set; }

        /// <summary>Calificación promedio de 0 a 5.</summary>
        public double Calificacion { get; set; }

        /// <summary>
        /// Especificaciones técnicas como pares clave-valor.
        /// Ejemplo: { "RAM": "16GB", "Procesador": "Intel i7" }
        /// </summary>
        public Dictionary<string, string> Especificaciones { get; set; } = new();
    }
}