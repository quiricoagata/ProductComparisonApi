using Microsoft.AspNetCore.Mvc;

namespace ProductComparisonApi.Domain.Models
{
    public class ComparisonRequest
    {
        [FromQuery(Name = "ids")]
        public List<int> ProductIds { get; set; } = new();
    }
}