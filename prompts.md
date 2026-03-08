# Prompts utilizados

Este archivo documenta los prompts utilizados durante el desarrollo del proyecto
para generación de código, tests, casos de prueba, documentación y diagramas con herramientas de IA.

---

## Herramientas utilizadas

- **GitHub Copilot** — Generación de código, tests, datos de prueba y diagramas
- **Claude (Anthropic)** — Documentación, decisiones arquitectónicas y configuración a modo de consulta

---

## 1. Generación de tests unitarios
**Herramienta:** GitHub Copilot  
**Archivos de referencia:** `#ProductController.cs` `#ProductService.cs` `#ProductRepository.cs` `#ProductValidator.cs` `#ProductsHealthCheck.cs` `#IProductService.cs` `#IProductRepository.cs` `#IProductValidator.cs` `#IJsonFileReader.cs` `#Product.cs` `#Response.cs` `#ComparisonRequest.cs` `#UpdateProductRequest.cs`
```
Generá tests unitarios completos para todos los archivos referenciados usando xUnit 2.x y Moq 4.x.

ESTRUCTURA:
ProductComparisonApi.Tests/
├── Controllers/ProductsControllerTests.cs              → namespace ProductComparisonApi.Tests.Controllers
├── Application/Services/ProductServiceTests.cs         → namespace ProductComparisonApi.Tests.Application.Services
├── Application/Services/ProductValidatorTests.cs       → namespace ProductComparisonApi.Tests.Application.Services
├── Infrastructure/Repositories/ProductRepositoryTests.cs → namespace ProductComparisonApi.Tests.Infrastructure.Repositories
└── Infrastructure/HealthChecks/ProductsHealthCheckTests.cs → namespace ProductComparisonApi.Tests.Infrastructure.HealthChecks

REGLAS GENERALES:
- Patrón Arrange / Act / Assert con comentarios explícitos
- Nombres en español: MetodoTesteado_Escenario_ResultadoEsperado
- Mockeá todas las dependencias, nunca uses implementaciones reales
- Helper CreateService() / CreateRepository() / CreateController() para instanciar el SUT

COBERTURA MÍNIMA: ProductValidator ~95% | Controller ~95% | ProductService ~95% | ProductRepository ~95% | HealthCheck ~95%
Cubrí: happy path, casos de error, casos límite, concurrencia y constructor de ProductRepository.

ESPECÍFICO POR CLASE:
- Controller: mockeá IProductService e IProductValidator. Verificá tipo de resultado y contenido de Response<T>.
- ProductService (Application): mockeá IProductRepository e ILogger.
  Verificá que cada método delega correctamente al repository.
  Cubrí: delegación exitosa, propagación de KeyNotFoundException y propagación de excepciones de I/O.
- ProductRepository (Infrastructure): mockeá ILogger, IWebHostEnvironment e IJsonFileReader.
  _fakePath = Path.Combine(AppContext.BaseDirectory, "Data", "products.json").
  Mockeá JsonPath en IJsonFileReader retornando _fakePath.
  Incluí tests de concurrencia, constructor y que WriteAllTextAsync se llama una vez por operación.
- ProductValidator: sin mocks. Casos límite: calificación 0 y 5, precio 0.01, strings vacíos y whitespace.
- HealthCheck: mockeá IProductService. Cubrí: true → Healthy, false → Unhealthy, excepción → Unhealthy.
```

---

## 2. Generación/Mejora del README
**Herramienta:** GitHub Copilot / Claude (Anthropic)  
**Archivos de referencia:** `#ProductController.cs` `#ProductService.cs` `#ProductRepository.cs` `#ProductValidator.cs` `#ProductsHealthCheck.cs` `#IProductService.cs` `#IProductRepository.cs` `#IProductValidator.cs` `#IJsonFileReader.cs` `#Product.cs` `#Response.cs` `#ComparisonRequest.cs` `#UpdateProductRequest.cs`
```
Generá un README.md completo y profesional. Extraé nombres de clases, rutas,
namespaces y configuraciones directamente del código. No inventes nada.

CONTEXTO:
- Nombre: Product Comparison API
- Tecnología: .NET 8, ASP.NET Core Web API, C# 12
- URL producción: [poner un ejemplo, despues lo reemplazo por la url real]
- Reporte de cobertura: [poner un ejemplo, despues lo reemplazo por la url real]

SECCIONES (en este orden):
1. Título + badges (versión, build, licencia, .NET)
2. Tabla de contenidos con links
3. Características — incluyendo que POST/PUT/PATCH/DELETE son extensión voluntaria, no requeridos por el challenge
4. Tecnologías — stack + tabla de proyectos + diagrama de dependencias
5. Requisitos previos
6. Instalación — pasos con bloques de código
7. Docker — build, run, volumen /app/Data, DATA_PATH, entrypoint.sh
8. Configuración — tabla de variables de entorno + ejemplo appsettings.Development.json
9. Demo — links a /swagger, /health, /api/products y reporte de cobertura
10. Uso — ejemplos curl de GET /api/products y GET /api/products/compare?ids=1&ids=3
11. Endpoints — tabla + ejemplos request/response + nota aclarando que GET /api/products,
    GET /api/products/{id} y GET /api/products/compare son el núcleo requerido, y que los
    endpoints de escritura fueron agregados intencionalmente para facilitar pruebas y
    demostrar conocimiento de CRUD completo. Esta decisión es consciente.
12. Autenticación — sin auth en v1, JWT planificado
13. Errores — formato Response<T> + tabla de códigos 400/404/500
14. Decisiones Arquitectónicas — Clean Architecture con 5 proyectos, dependencias forzadas por compilador,
    flujo Controller → IProductService → IProductRepository → IJsonFileReader → products.json,
    patrones (DI, Factory Method, Strategy, Repository, Service Layer, Null Object, Middleware),
    concurrencia con ConcurrentDictionary y SemaphoreSlim en ProductRepository (necesarios porque
    JSON no tiene control nativo de concurrencia, prescindibles al migrar a SQL Server)
15. Testing — estructura de carpetas, comandos dotnet test, cobertura esperada por clase,
    link al reporte, nota que Program.cs e interfaces están excluidos
16. Contribución — fork, rama, Conventional Commits, prefijos feat/fix/docs/test/refactor
17. Autora — [Nombre]

FORMATO:
- Bloques de código con lenguaje especificado (json, bash, csharp)
- Endpoints en código inline, métodos HTTP en negrita
- Variables de entorno en MAYUSCULAS
- Compatible con GitHub
```

---

## 3. Diagrama de arquitectura

**Herramienta:** GitHub Copilot  
**Archivos de referencia:** `#solution` `#ProductController.cs` `#ProductService.cs` `#ProductRepository.cs` `#JsonFileReader.cs` `#ProductsHealthCheck.cs` `#ProductValidator.cs`
```
Generá un diagrama Mermaid graph LR usando como referencia #solution
#ProductsController.cs #ProductService.cs #ProductRepository.cs
#JsonFileReader.cs #ProductsHealthCheck.cs #ProductValidator.cs #products.json
que muestre las capas de la aplicación de forma horizontal
con todas las conexiones y métodos intermedios.
```

---

## 4. Generación de datos de prueba
**Herramienta:** GitHub Copilot / Claude (Anthropic)  
**Archivos de referencia:** `#Product.cs` `#products.json`
```
Generá un archivo products.json con 10 productos de prueba realistas.
Tomá como referencia la estructura del modelo Product en #Product.cs
y el formato actual de #products.json.

REGLAS:
- Los IDs deben ser correlativos empezando desde 1
- Los productos deben ser laptops de distintas marcas y rangos de precio
- Precio: entre 500 y 3000, con decimales realistas
- Calificacion: entre 3.5 y 5.0, con un decimal
- Especificaciones: incluir Procesador, RAM, Almacenamiento, Pantalla,
  Batería y Sistema Operativo para todos los productos
- Los nombres, descripciones y especificaciones deben ser variados y representativos
```

---

## Nota

Todo el contenido generado por las herramientas de IA fue revisado, analizado y adaptado según los requerimientos reales del proyecto. No se utilizó ninguna respuesta de forma directa sin validación previa.

En particular:
- Los **tests unitarios** fueron implementados como base propia y luego se utilizó IA para ampliarlos y aumentar la cobertura, ajustando cada caso al comportamiento real de cada clase.
- La **documentación** fue generada como borrador y luego corregida, reestructurada y completada manualmente para reflejar con precisión las decisiones de diseño del proyecto.
- El **diagrama de arquitectura** fue ajustado para representar correctamente las dependencias y el flujo real de la aplicación.
- En todos los casos, la IA funcionó como herramienta de asistencia y no como reemplazo del criterio técnico propio.
