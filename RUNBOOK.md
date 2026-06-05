# Runbook de Rollback — Facturación Universal

Procedimiento a seguir cuando un deploy a producción falla o introduce regresiones.

---

## 1. Identificar la release anterior

```bash
# Ver historial de releases en Fly.io
flyctl releases list

# Alternativamente, identificar el commit/tag anterior en git
git log --oneline -10
git tag --sort=-creatordate | head -5
```

Anotar el **ID de imagen** de la release anterior (columna `IMAGE` en `flyctl releases list`).

---

## 2. Hacer rollback de la app en Fly.io

```bash
flyctl deploy --image <imagen-anterior>
```

Donde `<imagen-anterior>` es el ID obtenido en el paso anterior.

---

## 3. Evaluar compatibilidad de la migración de BD

Revisar si la versión anterior del código es compatible con el esquema actual de la base de datos:

- Si la migración nueva **solo agrega** columnas/tablas opcionales → la versión anterior probablemente funciona sin rollback de BD.
- Si la migración nueva **elimina columnas**, **cambia tipos**, o **rompe restricciones** que la versión anterior necesita → es necesario revertir la BD.

---

## 4. Revertir la BD (si es necesario)

Ir a **GitHub → Actions → Run EF Core Migrations** e iniciar el workflow manualmente con el campo `migration_target` apuntando a la migración **anterior** al deploy fallido.

```
migration_target: <NombreDeLaMigracionAnterior>
```

EF Core ejecutará `database update <NombreDeLaMigracionAnterior>`, revirtiendo las migraciones aplicadas de más.

---

## 5. Verificar que el servicio está sano

```bash
curl -i https://<dominio-de-la-app>/health
```

Debe responder `HTTP 200`. Si no responde en 60 segundos, revisar logs:

```bash
flyctl logs
```

---

## 6. Abrir issue de causa raíz

Crear un issue en GitHub con:

- **Título**: `[Incident] Deploy fallido — <fecha>`
- **Descripción**:
  - Release afectada (ID de imagen)
  - Síntoma observado (error, health check rojo, etc.)
  - Acción tomada (rollback a imagen X, rollback de BD a migración Y)
  - Causa raíz identificada
  - Acciones preventivas propuestas
