# RecetasApi

API REST en .NET 10 con CRUD de **alimentos** y **recetas** (relación muchos a muchos con cantidad y unidad de medida).

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
