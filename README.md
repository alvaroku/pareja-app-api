# ParejaApp API

API REST moderna construida con .NET 8.0 para gestionar relaciones de pareja a través de citas, metas y memorias compartidas.

## Descripción

ParejaApp API es el backend de la plataforma integral diseñada para ayudar a las parejas a organizar y mantener viva su relación. La API proporciona:

- **Autenticación y Autorización**: Sistema completo con JWT para gestión segura de usuarios.
- **Gestión de Citas**: Endpoints para crear, actualizar, listar y eliminar citas románticas con fechas y ubicaciones.
- **Metas Compartidas**: API para definir objetivos en pareja con seguimiento de progreso y estados.
- **Memorias**: Almacenamiento y gestión de momentos especiales con soporte para imágenes en Firebase Storage.
- **Gestión de Pareja**: Sistema de invitaciones y conexión entre usuarios.
- **Subida de Archivos**: Integración con Firebase Storage para gestión de imágenes de perfil y memorias.

### Funcionalidades Próximas

- **Recordatorios de Citas**: Sistema de Jobs programados para envío de notificaciones por email.
- **Verificación de Correo**: Confirmación de email mediante tokens de activación.
- **Recuperación de Contraseña**: Sistema de restablecimiento mediante email con tokens temporales.
- **Health Checks Avanzados**: Monitoreo y servicios externos.
- **Rate Limiting**: Protección contra abuso de endpoints.

### Tecnologías

- **Framework**: .NET 8.0 (Minimal API)
- **ORM**: Entity Framework Core 8.0
- **Base de Datos**: SQL Server
- **Autenticación**: JWT Bearer
- **Storage**: Firebase Storage
- **Email**: SMTP (configurable)
- **Seguridad**: BCrypt.Net para hashing de contraseñas

## Instalación

### Prerrequisitos

- .NET 8.0 SDK o superior
- SQL Server 2019 o superior (o SQL Server Express)
- Cuenta de Firebase (para storage de imágenes)
- Cliente SQL (SQL Server Management Studio, Azure Data Studio, etc.)

### Pasos de instalación

1. Clona el repositorio:
```bash
git clone https://github.com/alvaroku/pareja-app-web.git
cd ParejaAppAPI
```

2. Restaura las dependencias:
```bash
dotnet restore
```

3. Configura la cadena de conexión en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ParejaAppDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

4. Configura las credenciales de Firebase:
   - Crea un proyecto en Firebase Console
   - Descarga el archivo de credenciales JSON
   - Conviertelo a base 64 y colócalo en Firebase:Credential, tambien necesitarás el nombre del bucket en Firebase:StorageBucket `appsettings.json`

5. Crea la base de datos con migraciones:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

## Ejecución

### Servidor de desarrollo

Para iniciar la API en modo desarrollo:

```bash
dotnet run
```

La API estará disponible en:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`
- Swagger UI: `https://localhost:5001/swagger`

### Build de producción

Para compilar el proyecto en modo release:

```bash
dotnet build --configuration Release
```

Para publicar la aplicación:

```bash
dotnet publish --configuration Release --output ./publish
```

Los archivos publicados se generarán en el directorio `./publish/`

## Estructura del Proyecto

```
ParejaAppAPI/
├── Data/
│   └── AppDbContext.cs          # Contexto de Entity Framework
├── Endpoints/
│   ├── AuthEndpoints.cs         # Endpoints de autenticación
│   ├── CitaEndpoints.cs         # Endpoints de citas
│   ├── MetaEndpoints.cs         # Endpoints de metas
│   ├── MemoriaEndpoints.cs      # Endpoints de memorias
│   ├── ParejaEndpoints.cs       # Endpoints de gestión de pareja
│   └── UsuarioEndpoints.cs      # Endpoints de usuarios
├── Models/
│   ├── DTOs/                    # Data Transfer Objects
│   ├── Entities/                # Entidades de base de datos
│   └── Responses/               # Modelos de respuesta
├── Repositories/
│   ├── Interfaces/              # Contratos de repositorios
│   └── *Repository.cs           # Implementaciones
├── Services/
│   ├── Interfaces/              # Contratos de servicios
│   ├── AuthService.cs           # Lógica de autenticación
│   ├── EmailService.cs          # Envío de emails
│   ├── FirebaseStorageService.cs # Gestión de archivos
│   └── *Service.cs              # Servicios de negocio
├── Migrations/                  # Migraciones de EF Core
├── appsettings.json             # Configuración de desarrollo
└── Program.cs                   # Punto de entrada
```

## Endpoints Disponibles

### Autenticación (Públicos)

```
POST /api/auth/register          # Registro de nuevo usuario
POST /api/auth/login             # Login con email/password
```

### Usuarios (Autenticados)

```
GET    /api/usuarios/{id}        # Obtener usuario por ID
PUT    /api/usuarios/{id}        # Actualizar usuario
DELETE /api/usuarios/{id}        # Eliminar usuario
POST   /api/usuarios/{id}/foto   # Subir foto de perfil
DELETE /api/usuarios/{id}/foto   # Eliminar foto de perfil
```

### Pareja (Autenticados)

```
POST   /api/pareja/invitar                    # Enviar invitación
POST   /api/pareja/aceptar/{invitacionId}     # Aceptar invitación
POST   /api/pareja/rechazar/{invitacionId}    # Rechazar invitación
DELETE /api/pareja/desvincular                # Desvincular pareja
GET    /api/pareja/activa/{usuarioId}         # Obtener pareja activa
GET    /api/pareja/invitaciones/{usuarioId}   # Listar invitaciones
```

### Citas (Autenticados)

```
GET    /api/citas/{id}                        # Obtener cita por ID
GET    /api/citas/usuario/{usuarioId}         # Citas de usuario
POST   /api/citas                             # Crear cita
PUT    /api/citas/{id}                        # Actualizar cita
DELETE /api/citas/{id}                        # Eliminar cita
```

### Metas (Autenticados)

```
GET    /api/metas/{id}                        # Obtener meta por ID
GET    /api/metas/usuario/{usuarioId}         # Metas de usuario
POST   /api/metas                             # Crear meta
PUT    /api/metas/{id}                        # Actualizar meta
DELETE /api/metas/{id}                        # Eliminar meta
```

### Memorias (Autenticados)

```
GET    /api/memorias/{id}                     # Obtener memoria por ID
GET    /api/memorias/usuario/{usuarioId}      # Memorias de usuario
POST   /api/memorias                          # Crear memoria
PUT    /api/memorias/{id}                     # Actualizar memoria
DELETE /api/memorias/{id}                     # Eliminar memoria
POST   /api/memorias/{id}/foto                # Subir foto de memoria
DELETE /api/memorias/{id}/foto                # Eliminar foto de memoria
```

### Health Checks

```
GET /health                                    # Health check general
GET /health/ready                              # Readiness probe
GET /health/live                               # Liveness probe
```

## Autenticación

La API utiliza JWT (JSON Web Tokens) para autenticación. 

### Obtener Token

```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "usuario@ejemplo.com",
  "password": "tuPassword123"
}
```

Respuesta exitosa:
```json
{
  "isSuccess": true,
  "statusCode": 200,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "usuario": {
      "id": 1,
      "nombre": "Usuario",
      "email": "usuario@ejemplo.com"
    }
  },
  "message": "Login exitoso"
}
```

### Usar Token

Incluye el token en el header `Authorization` de todas las peticiones protegidas:

```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Configuración JWT

El token se configura en `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "tu-clave-secreta-super-segura-de-al-menos-32-caracteres",
    "Issuer": "ParejaAppAPI",
    "Audience": "ParejaAppClient",
  }
}
```

**IMPORTANTE**: Cambia `SecretKey` antes de desplegar a producción.

## Patrón de Respuesta

Todas las respuestas de la API utilizan el modelo `Response<T>`:

### Respuesta Exitosa

```json
{
  "isSuccess": true,
  "statusCode": 200,
  "data": { /* datos solicitados */ },
  "message": "Operación exitosa",
  "errors": null
}
```

### Respuesta con Error

```json
{
  "isSuccess": false,
  "statusCode": 404,
  "data": null,
  "message": "Recurso no encontrado",
  "errors": ["Detalle específico del error"]
}
```

### Códigos de Estado HTTP

- `200 OK` - Operación exitosa
- `201 Created` - Recurso creado exitosamente
- `204 No Content` - Operación exitosa sin contenido
- `400 Bad Request` - Datos inválidos
- `401 Unauthorized` - Token inválido o ausente
- `403 Forbidden` - Sin permisos suficientes
- `404 Not Found` - Recurso no encontrado
- `500 Internal Server Error` - Error del servidor

## Migraciones de Base de Datos

### Crear nueva migración

```bash
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar migraciones

```bash
dotnet ef database update
```

### Revertir última migración

```bash
dotnet ef database update NombreMigracionAnterior
```

### Eliminar última migración

```bash
dotnet ef migrations remove
```

## Configuración de Email

Configura el servicio SMTP en `appsettings.json`:

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@parejaapp.com",
    "SenderName": "ParejaApp",
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password",
    "EnableSsl": true
  }
}
```

## Despliegue

### Azure App Service

1. Crea un App Service en Azure Portal
2. Configura la cadena de conexión en Configuration
3. Publica usando CLI:

```bash
dotnet publish -c Release
```

4. Despliega los archivos desde `bin/Release/net8.0/publish/`

### Docker

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ParejaAppAPI.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ParejaAppAPI.dll"]
```

Construir y ejecutar:

```bash
docker build -t parejaapp-api .
docker run -p 5000:80 -p 5001:443 parejaapp-api
```

## Scripts Disponibles

```bash
dotnet run                       # Ejecutar en desarrollo
dotnet build                     # Compilar proyecto
dotnet test                      # Ejecutar tests
dotnet ef migrations add [name]  # Crear migración
dotnet ef database update        # Aplicar migraciones
dotnet publish -c Release        # Publicar para producción
```

## Seguridad

### Mejores Prácticas Implementadas

- Contraseñas hasheadas con BCrypt (cost factor 11)
- Tokens JWT con expiración configurable
- CORS configurado para orígenes específicos
- HTTPS habilitado por defecto
- Validación de entrada en todos los DTOs
- Soft delete para mantener integridad de datos
- Health checks para monitoreo

### Configuración de Producción

Antes de desplegar a producción:

1. Cambia `Jwt:SecretKey` a una clave segura de 32+ caracteres
2. Actualiza `AllowedOrigins` en CORS
3. Configura certificados SSL válidos
4. Habilita logging en producción
5. Configura backup automático de base de datos
6. Implementa rate limiting
