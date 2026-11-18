# RecetasApi

API REST en .NET 10 con CRUD de **alimentos** y **recetas** (relación muchos a muchos con cantidad y unidad).

## Cómo ejecutarlo

Requisitos: .NET 10 SDK.

```bash
dotnet tool install --global dotnet-ef --version 10.*   # solo la primera vez
dotnet restore
dotnet ef migrations add Inicial                        # solo si la carpeta Migrations no existe
dotnet ef database update                               # crea recetas.db con datos de ejemplo
dotnet run                                              # http://localhost:5080
```

Pruebas manuales: `RecetasApi.http` (extensión REST Client de VS Code) o el documento OpenAPI en `/openapi/v1.json`.

## Endpoints

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/alimentos?q=&categoria=&page=&pageSize=` | Listado paginado y filtrable |
| GET / PUT / DELETE | `/api/alimentos/{id}` | Detalle, reemplazo, borrado (409 si está en una receta) |
| POST | `/api/alimentos` | Alta (409 si el nombre ya existe) |
| GET | `/api/recetas?q=&dificultad=&alimento=&page=&pageSize=` | Listado paginado (resumen) |
| GET | `/api/recetas/{id}` | Detalle con ingredientes y calorías totales / por porción |
| POST / PUT | `/api/recetas` , `/api/recetas/{id}` | Alta y reemplazo, con ingredientes anidados |
| DELETE | `/api/recetas/{id}` | Borrado (elimina también sus ingredientes) |

## Decisiones de diseño

- **Controladores + servicios + DbContext**: capas suficientes para un CRUD pequeño sin sobre-ingeniería.
- **DTOs separados de las entidades**: evitan ciclos en el JSON (Receta → Ingredientes → Alimento) y desacoplan la API de la base de datos.
- **SQLite + EF Core**: se ejecuta sin instalar nada; cambiar de proveedor es cuestión de configuración.
- **Enums como texto** en la API y en la base de datos.
- **Errores en un único sitio** (`GlobalExceptionHandler`) con formato ProblemDetails.
- **Fechas de auditoría en UTC**, rellenadas en `SaveChangesAsync`.
- **Listado de recetas ligero**: proyección directa a un DTO resumen, sin cargar ingredientes.
- **Cálculo nutricional aislado** en `CalculoNutricional`, funciones puras fáciles de testear.

## Qué le falta para producción

- Autenticación y autorización.
- Tests automáticos (unitarios y de integración).
- Base de datos real (PostgreSQL / SQL Server) y migraciones aplicadas desde el pipeline de despliegue.
- Concurrencia optimista (token de versión) para ediciones simultáneas.
- Gestión de la condición de carrera en nombres únicos (capturar la violación del índice, no solo comprobar antes).
- Logging estructurado, health checks, métricas y trazas.
- CORS, HTTPS/HSTS, rate limiting y versionado de la API.
- Conversión de unidades real (la API trata 1 ml como 1 g).
- Dockerfile y CI/CD.
