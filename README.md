# 🛒 Product Comparison API

[![Version](https://img.shields.io/badge/version-1.0.0-blue.svg)](https://semver.org/)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen.svg)](https://github.com)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![.NET Version](https://img.shields.io/badge/.NET-8.0-512BD4.svg)](https://dotnet.microsoft.com/)

**API RESTful para consultar, comparar y gestionar productos.** Expone endpoints para obtener detalles de productos, comparar múltiples items lado a lado y realizar operaciones CRUD completas. La persistencia de datos se simula mediante un archivo `products.json` local que actúa como base de datos.

---

## 📋 Tabla de contenidos

- [Características](#-características)
- [Tecnologías](#-tecnologías)
- [Requisitos previos](#-requisitos-previos)
- [Instalación](#-instalación)
- [Docker](#-docker)
- [Configuración](#-configuración)
- [Demo](#-demo)
- [Uso](#-uso)
- [Endpoints](#-endpoints)
- [Autenticación](#-autenticación)
- [Errores](#-errores)
- [Decisiones Arquitectónicas y Patrones de Diseño](#-decisiones-arquitectónicas-y-patrones-de-diseño)
- [Testing](#-testing)
- [Contribución](#-contribución)
- [Autora](#-autora)
- [Licencia](#-licencia)

---

## ✨ Características

- ✅ **Consulta del catálogo completo** — Obtén todos los productos disponibles en un único endpoint
- ✅ **Detalle de producto por ID** — Accede a la información completa de un producto específico
- ✅ **Comparación de múltiples productos** — Compara hasta N productos lado a lado con `GET /api/products/compare?ids=1&ids=2`
- ✅ **Creación de productos** — Agrega nuevos productos con ID autogenerado (POST)
- ✅ **Actualización completa** — Reemplaza todos los campos de un producto (PUT)
- ✅ **Actualización parcial** — Modifica solo los campos enviados en el body (PATCH)
- ✅ **Eliminación de productos** — Elimina permanentemente un producto del catálogo (DELETE)
- ✅ **Persistencia simulada** — Los cambios se guardan en un archivo JSON local en `Infrastructure/Data/` y **sobreviven al reinicio**
- ✅ **Respuestas con envoltorio genérico** — Formato consistente `Response<T>` con campos `success`, `message` y `data`
- ✅ **Manejo centralizado de errores** — Códigos HTTP semánticos (400, 404, 500) con mensajes descriptivos
- ✅ **Documentación interactiva** — Swagger / OpenAPI disponible en `/swagger` (incluso en producción)
- ✅ **Health check genérico** — Endpoint `/health` que verifica la fuente de datos a través de `IProductService.IsHealthyAsync()`
- ✅ **Clean Architecture** — 4 proyectos separados: Domain, Application, Infrastructure y API con dependencias forzadas por el compilador
- ✅ **Interfaces en todas las dependencias** — Facilita el mockeo en tests unitarios con xUnit y Moq
- ✅ **Concurrencia thread-safe** — `ConcurrentDictionary` y `SemaphoreSlim` para acceso seguro desde múltiples requests
- ✅ **Deploy containerizado** — Docker multi-stage con Railway y volumen persistente `/app/Data`
- ✅ **Endpoints de administración** — POST, PUT, PATCH, DELETE incluidos como extensión voluntaria para facilitar las pruebas y demostrar conocimiento de CRUD completo. No son requeridos por el enunciado del challenge.

---

## 🛠️ Tecnologías

### 🧰 Stack utilizado

| Tecnología | Versión | Propósito |
|-----------|---------|----------|
| **.NET** | 8.0 | Runtime y framework |
| **C#** | 12.0 | Lenguaje de programación |
| **ASP.NET Core** | 8.0 | Web API y controllers |
| **Swagger / Swashbuckle** | 6.x | Documentación interactiva OpenAPI |
| **xUnit** | 2.x | Framework de testing unitario |
| **Moq** | 4.x | Mocking de dependencias en tests |
| **System.Text.Json** | Built-in | Serialización JSON |
| **Docker** | Latest | Containerización |
| **Railway** | Cloud | Hosting en producción |

### 🗂️ Proyectos de la solución

| Proyecto | Tipo | Descripción |
|----------|------|-------------|
| **ProductComparisonApi.Domain** | Class Library | Modelos e interfaces sin dependencias externas. Núcleo del dominio. |
| **ProductComparisonApi.Application** | Class Library | Lógica de negocio y validaciones (`ProductValidator`). |
| **ProductComparisonApi.Infrastructure** | Class Library | Acceso a datos (`ProductService`, `JsonFileReader`), sistema de archivos y health checks. |
| **ProductComparisonApi.API** | ASP.NET Core Web API | Controladores REST, configuración de servicios y punto de entrada. |
| **ProductComparisonApi.Tests** | xUnit Test Project | Tests unitarios de todas las capas con Moq. |

### 🧭 Diagrama de dependencias

```
Tests ──────────┐
                ├──> API ──────────┐
                │                 ├──> Infrastructure ──┐
                ├──> Infrastructure    Application      ├──> Domain
                │                                      │
                ├──> Application ─────────────────────┘
                │
                └──> Domain (sin dependencias externas)
```

Las dependencias son forzadas por el compilador: si intentas importar desde un proyecto de nivel superior a uno inferior, obtendrás error de compilación.

---

## ✅ Requisitos previos

- **.NET 8 SDK** o superior — [Descargar](https://dotnet.microsoft.com/download)
- **Visual Studio 2022** (17.8+) o **Visual Studio Code**
- **Git**
- **Docker** (opcional, para correr en contenedor)

---

## ⚙️ Instalación

### 1. 📥 Clonar el repositorio

```bash
git clone https://github.com/usuario/ProductComparisonApi.git
cd ProductComparisonApi
```

### 2. 🔁 Restaurar dependencias

```bash
dotnet restore
```

### 3. 🔎 Verificar configuración de archivos

Asegúrate de que `ProductComparisonApi.Infrastructure/Data/products.json` esté presente y configurado con **"Copiar siempre"** en Visual Studio:

- Click derecho en `products.json` → **Propiedades** → **Copiar en directorio de salida: Copiar siempre**

### 4. 🛠️ Compilar la solución

```bash
dotnet build
```

### 5. ▶️ Ejecutar la API en desarrollo

```bash
dotnet run --project ProductComparisonApi.API
```

### 6. 🧭 Acceder a Swagger

```
http://localhost:5000/swagger
```

---

## 🐳 Docker

### 🏗️ Buildear la imagen

```bash
docker build -t productcomparisonapi:latest .
```

### ▶️ Ejecutar el contenedor

```bash
docker run -p 8080:8080 productcomparisonapi:latest
```

### 💾 Volumen persistente

En producción en Railway, se configura un volumen montado en `/app/Data` que persiste el archivo `products.json` entre despliegues:

```bash
docker run -p 8080:8080 -v productos-volume:/app/Data productcomparisonapi:latest
```

**Configuración en Railway:**
- **Mount Path:** `/app/Data`
- **Variable de entorno:** `DATA_PATH=/app`

### 🛠️ Inicialización automática

El script `entrypoint.sh` verifica si el archivo existe en el volumen. Si es el primer deploy, copia el backup desde `products.json.default`:

```bash
#!/bin/bash
if [ ! -f "/app/Data/products.json" ]; then
    mkdir -p /app/Data
    cp /app/products.json.default /app/Data/products.json
fi
exec dotnet ProductComparisonApi.API.dll
```

---

## 🔧 Configuración

### 🔐 Variables de entorno

| Variable | Descripción | Obligatoria | Valor por defecto |
|----------|-------------|-------------|-------------------|
| `ASPNETCORE_ENVIRONMENT` | Entorno de ejecución (`Development`, `Production`) | No | `Production` |
| `ASPNETCORE_URLS` | URL y puerto de escucha | No | `http://+:8080` |
| `DATA_PATH` | Ruta base del archivo JSON de productos | No | `AppContext.BaseDirectory` |

### 📝 Ejemplo de `appsettings.Development.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

---

## 🌐 Demo

### 🚀 URL de producción

```
https://productcomparisonapi-production.up.railway.app
```

### 🔗 Links directos

- 📖 **Swagger UI:** https://productcomparisonapi-production.up.railway.app/swagger
- 💚 **Health Check:** https://productcomparisonapi-production.up.railway.app/health
- 📦 **Todos los productos:** https://productcomparisonapi-production.up.railway.app/api/products

---

## 🧭 Uso

### 🧑‍💻 En desarrollo

```bash
dotnet run --project ProductComparisonApi.API
```

### 📥 Ejemplo: Obtener todos los productos

```bash
curl -X GET "https://productcomparisonapi-production.up.railway.app/api/products" \
  -H "Content-Type: application/json"
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "4 productos encontrados.",
  "data": [
    {
      "id": 1,
      "nombre": "Laptop Pro X1",
      "urlImagen": "https://placehold.co/400x300?text=Laptop+Pro+X1",
      "descripcion": "Laptop de alto rendimiento para profesionales creativos.",
      "precio": 1299.99,
      "calificacion": 4.7,
      "especificaciones": {
        "Procesador": "Intel Core i7-13th Gen",
        "RAM": "16 GB DDR5"
      }
    }
  ]
}
```

### 🔍 Ejemplo: Comparar productos

```bash
curl -X GET "https://productcomparisonapi-production.up.railway.app/api/products/compare?ids=1&ids=3" \
  -H "Content-Type: application/json"
```

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Comparando 2 productos.",
  "data": [
    { "id": 1, "nombre": "Laptop Pro X1", "precio": 1299.99, "calificacion": 4.7 },
    { "id": 3, "nombre": "Gaming Beast G9", "precio": 1899.99, "calificacion": 4.9 }
  ]
}
```

---

## 🔌 Endpoints

### 📚 Resumen

| Método | Ruta | Descripción | Categoría |
|--------|------|-------------|-----------|
| **GET** | `/api/products` | Obtiene todos los productos | Consulta |
| **GET** | `/api/products/{id}` | Obtiene un producto por ID | Consulta |
| **GET** | `/api/products/compare` | Compara múltiples productos | Consulta |
| **POST** | `/api/products` | Crea un nuevo producto | Administración |
| **PUT** | `/api/products/{id}` | Actualiza un producto completo | Administración |
| **PATCH** | `/api/products/{id}` | Actualiza parcialmente un producto | Administración |
| **DELETE** | `/api/products/{id}` | Elimina un producto | Administración |
| **GET** | `/health` | Verifica la salud de la API | Monitoreo |

> **Nota:** Los endpoints **GET** `/api/products`, **GET** `/api/products/{id}` y
> **GET** `/api/products/compare` constituyen el núcleo funcional requerido por el challenge.
> Los endpoints de escritura (**POST**, **PUT**, **PATCH**, **DELETE**) fueron agregados de forma
> intencional por dos razones: facilitar las pruebas de la API sin necesidad de herramientas
> externas, y demostrar conocimiento de CRUD completo, manejo de concurrencia, validaciones
> parciales con PATCH y cobertura de tests unitarios sobre operaciones de escritura.
> Esta decisión es consciente y no implica una malinterpretación del objetivo del challenge.

### 📖 GET /api/products

**Response (200 OK):**

```json
{
  "success": true,
  "message": "4 productos encontrados.",
  "data": [ { "id": 1, "nombre": "Laptop Pro X1", "precio": 1299.99, "calificacion": 4.7 } ]
}
```

### 🔎 GET /api/products/{id}

**Response (200 OK):**

```json
{
  "success": true,
  "message": null,
  "data": { "id": 1, "nombre": "Laptop Pro X1", "precio": 1299.99, "calificacion": 4.7 }
}
```

**Response (404 Not Found):**

```json
{ "success": false, "message": "No existe un producto con ID 99.", "data": null }
```

### 🔄 GET /api/products/compare

**Request:** `GET /api/products/compare?ids=1&ids=3`

**Response (200 OK):**

```json
{
  "success": true,
  "message": "Comparando 2 productos.",
  "data": [
    { "id": 1, "nombre": "Laptop Pro X1", "precio": 1299.99 },
    { "id": 3, "nombre": "Gaming Beast G9", "precio": 1899.99 }
  ]
}
```

**Response (400 Bad Request) — Menos de 2 IDs:**

```json
{ "success": false, "message": "Debes enviar al menos 2 IDs de productos.", "data": null }
```

**Response (400 Bad Request) — IDs duplicados:**

```json
{ "success": false, "message": "La lista contiene IDs duplicados.", "data": null }
```

### ➕ POST /api/products

**Request:**

```json
{
  "nombre": "Laptop Nueva",
  "urlImagen": "https://img.com/nueva.jpg",
  "descripcion": "Laptop de última generación",
  "precio": 799.99,
  "calificacion": 4.2,
  "especificaciones": { "RAM": "8GB", "Almacenamiento": "256GB SSD" }
}
```

**Response (201 Created):**

```json
{
  "success": true,
  "message": "Producto creado correctamente.",
  "data": { "id": 5, "nombre": "Laptop Nueva", "precio": 799.99, "calificacion": 4.2 }
}
```

### 🛠️ PUT /api/products/{id}

**Request:** Igual que POST con todos los campos requeridos.

**Response (200 OK):**

```json
{ "success": true, "message": "Producto actualizado correctamente.", "data": { "id": 1 } }
```

### ✏️ PATCH /api/products/{id}

**Request (solo los campos a modificar):**

```json
{ "precio": 1199.99 }
```

**Response (200 OK):**

```json
{ "success": true, "message": "Producto actualizado parcialmente.", "data": { "id": 1, "precio": 1199.99 } }
```

### 🗑️ DELETE /api/products/{id}

**Response (200 OK):**

```json
{ "success": true, "message": "Producto 1 eliminado correctamente.", "data": null }
```

### 💚 GET /health

**Response (200 OK):**

```json
{
  "status": "Healthy",
  "duration": "00:00:00.0234567",
  "entries": {
    "fuente_de_datos": {
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0150000"
    }
  }
}
```

---

## 🔐 Autenticación

La versión actual **no requiere autenticación**. Todos los endpoints son públicos.

En versiones futuras se implementaría **JWT (JSON Web Tokens)** para autenticación stateless y roles para proteger los endpoints de administración.

---

## ❌ Errores

### 📄 Formato estándar

```json
{
  "success": false,
  "message": "Descripción del error",
  "data": null
}
```

### ⚠️ Códigos de error HTTP

| Código | Descripción |
|--------|-------------|
| **400** | ID inválido — debe ser mayor a 0 |
| **400** | Campo requerido vacío |
| **400** | Precio inválido — debe ser mayor a 0 |
| **400** | Calificación fuera de rango — debe estar entre 0 y 5 |
| **400** | Menos de 2 IDs en comparación |
| **400** | IDs duplicados en comparación |
| **404** | Producto no encontrado |
| **404** | Uno o más IDs no existen |
| **500** | Error interno del servidor |

---

## 🏗️ Decisiones Arquitectónicas y Patrones de Diseño

### 🧱 Clean Architecture

La solución implementa **Clean Architecture** dividiendo la aplicación en 4 proyectos con responsabilidades claramente separadas. Las dependencias entre capas son **forzadas por el compilador**, no solo por convención. Si se intenta importar desde un proyecto de nivel superior a uno inferior, el compilador lanza error.

### 📂 Ubicación de `products.json`

El archivo `products.json` vive en `ProductComparisonApi.Infrastructure/Data/` porque simula la base de datos y su acceso es responsabilidad de la capa de infraestructura. En Docker se copia al volumen montado en `/app/Data`.

### ♟️ Patrones implementados

#### 🔌 Dependency Injection

Todos los servicios se registran en `Program.cs` y se inyectan por constructor:

```csharp
builder.Services.AddSingleton<IJsonFileReader, JsonFileReader>();
builder.Services.AddSingleton<IProductService, ProductService>();
builder.Services.AddScoped<IProductValidator, ProductValidator>();
```

#### 🏭 Factory Method

`Response<T>` expone métodos estáticos para crear respuestas sin exponer el constructor:

```csharp
Response<Product>.Ok(product, "Producto encontrado.");
Response<object>.Fail("No existe un producto con ese ID.");
Response<object>.Empty("Producto eliminado correctamente.");
```

#### 🔄 Strategy

`IProductService` e `IJsonFileReader` permiten intercambiar implementaciones sin tocar el controlador. Si se migra a SQL Server, solo cambia la implementación registrada en `Program.cs`.

#### 🗄️ Repository (parcial)

`ProductService` centraliza todo el acceso a datos — lectura, escritura y persistencia. El resto de la aplicación no sabe cómo ni dónde están almacenados los productos.

#### ➖ Null Object

`Response<object>.Empty()` devuelve un objeto válido en operaciones exitosas sin datos (como DELETE), evitando que el cliente reciba `null`.

#### 🧩 Decorator / Middleware

El health check actúa como decorador del pipeline HTTP, verificando la disponibilidad de la fuente de datos sin afectar la lógica de negocio.

### ⚙️ Concurrencia

#### 🧵 ConcurrentDictionary

```csharp
private readonly ConcurrentDictionary<int, Product> _products = new();
```

Reemplaza `List<T>` para garantizar acceso thread-safe desde múltiples requests simultáneos sin locks explícitos.

#### 🔒 SemaphoreSlim

```csharp
private readonly SemaphoreSlim _writeLock = new(1, 1);
```

Serializa las escrituras al archivo JSON para evitar que dos requests escriban simultáneamente y corrompan el archivo.

Ambos mecanismos fueron incorporados deliberadamente porque al usar un **archivo JSON** 
como capa de persistencia no se cuenta con los controles nativos de una base de datos 
relacional. Al migrar a **SQL Server** serían prescindibles, ya que el motor maneja la 
concurrencia y las transacciones ACID de forma automática.

---

## 🧪 Testing

### 🧩 Estructura

```
ProductComparisonApi.Tests/
├── Controllers/
│   └── ProductsControllerTests.cs
└── Infrastructure/
    ├── Services/
    │   ├── ProductServiceTests.cs
    │   └── ProductValidatorTests.cs
    └── HealthChecks/
        └── ProductsHealthCheckTests.cs
```

### ▶️ Ejecutar tests

```bash
# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test ProductComparisonApi.Tests

# Filtrar por clase
dotnet test --filter "ClassName~ProductValidatorTests"

# Con output verbose
dotnet test --verbosity detailed
```

### 🧾 Generar reporte de cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
reportgenerator -reports:"coverage.opencover.xml" -targetdir:"coveragereport"
```

📊 **Reporte de cobertura:** https://quiricoagata.github.io/ProductComparisonApi/coverage/index.html

### 🎯 Cobertura esperada

| Componente | Cobertura |
|-----------|----------|
| **ProductValidator** | ~95% |
| **ProductsController** | ~100% |
| **ProductService** | ~100% |
| **JsonFileReader** | ~100% |
| **ProductsHealthCheck** | ~100% |
| **Program.cs** | Excluido (`[ExcludeFromCodeCoverage]`) |
| **Interfaces** | Excluido |

---

## 🤝 Contribución

1. Fork del repositorio
2. Crear rama: `git checkout -b feat/nueva-funcionalidad`
3. Commit con Conventional Commits: `git commit -m "feat: descripción"`
4. Push: `git push origin feat/nueva-funcionalidad`
5. Abrir Pull Request

### 🏷️ Prefijos de commits

| Prefijo | Uso |
|---------|-----|
| `feat` | Nueva funcionalidad |
| `fix` | Corrección de bug |
| `docs` | Documentación |
| `test` | Tests |
| `refactor` | Refactorización |

---

## 👩‍💻 Autora

**Ágata Quirico**

Desarrollado como parte de un challenge técnico de backend.

---

## 📄 Licencia

Este proyecto está bajo la licencia MIT. Consulta el archivo `LICENSE` para más detalles.

---

## 📞 Soporte

Para reportar bugs o hacer sugerencias, por favor abre un issue en el repositorio.

---

**¡Gracias por usar Product Comparison API!** 🚀
