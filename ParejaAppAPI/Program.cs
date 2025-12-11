using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using ParejaAppAPI.Data;
using ParejaAppAPI.Endpoints;
using ParejaAppAPI.Repositories;
using ParejaAppAPI.Repositories.Interfaces;
using ParejaAppAPI.Services;
using ParejaAppAPI.Services.BackgroundServices;
using ParejaAppAPI.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Health Checks
builder.Services.AddHealthChecks();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy
            .WithOrigins(builder.Configuration["Cors:AllowedOrigins"].Split(","))
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});


// Repositories
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<ICitaRepository, CitaRepository>();
builder.Services.AddScoped<IMetaRepository, MetaRepository>();
builder.Services.AddScoped<IMemoriaRepository, MemoriaRepository>();
builder.Services.AddScoped<IParejaRepository, ParejaRepository>();
builder.Services.AddScoped<IResourceRepository, ResourceRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();

// Services
builder.Services.AddScoped<JwtService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IStorageService, FirebaseStorageService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<IMetaService, MetaService>();
builder.Services.AddScoped<IMemoriaService, MemoriaService>();
builder.Services.AddScoped<IParejaService, ParejaService>();
builder.Services.AddScoped<IResourceService, ResourceService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddHostedService<NotificationDispatcherWorker>();

builder.Services.AddScoped<IPushNotificationService, FirebasePushNotificationService>();
builder.Services.AddScoped<ISMSService, TwilioSMSService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();

// Health Checks
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapHealthChecks("/health/live");

// Map endpoints
app.MapAuthEndpoints();
app.MapUsuarioEndpoints();
app.MapParejaEndpoints();
app.MapCitaEndpoints();
app.MapMetaEndpoints();
app.MapMemoriaEndpoints();
app.MapNotificationEndpoints();

// Ejecutar migraciones autom�ticas al arrancar (sincr�nico para evitar cambiar la firma de Main)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.Migrate();
        logger.LogInformation("Se aplicaron las migraciones pendientes correctamente.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error al aplicar migraciones de la base de datos.");
        throw;
    }
}

app.Run();
