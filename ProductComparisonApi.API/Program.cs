using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ProductComparisonApi.Application.Services;
using ProductComparisonApi.Domain.Interfaces;
using ProductComparisonApi.Infrastructure.HealthChecks;
using ProductComparisonApi.Infrastructure.Services;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IJsonFileReader, JsonFileReader>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddScoped<IProductValidator, ProductValidator>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Product Comparison API",
        Version = "v1",
        Description = "API para consultar y comparar productos por sus características."
    });
    c.EnableAnnotations();
    var xmlFile = "ProductComparisonApi.API.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddHealthChecks()
    .AddCheck<ProductsHealthCheck>(
        name: "fuente_de_datos",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "storage" });

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.ToString(),
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.ToString()
                })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.UseCors();
app.UseAuthorization();
app.MapControllers();
app.Run();

[ExcludeFromCodeCoverage]
public partial class Program { }