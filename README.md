# Facturacion-Universal

API .NET 8 para facturacion electronica SRI Ecuador.

## Estado de conexion

- Supabase conectado y verificado contra el proyecto `vqrhoihdhilhktgqdnxn`.
- EF Core usa PostgreSQL/Supabase mediante `ConnectionStrings:DefaultConnection`.
- La cadena de conexion local esta configurada con `dotnet user-secrets` en `Facturacion.Api`; no se guarda password en `appsettings.json`.
- La migracion inicial `20260511150330_InitialCreate` quedo registrada como aplicada en `public.__EFMigrationsHistory`.
- `dotnet ef database update` termino correctamente y la API compila.

## Base de datos

La app usa el schema `facturacion` en Supabase. Las tablas existentes fueron conservadas; se completaron las columnas faltantes necesarias para que coincidan con el modelo EF Core actual.
