using System.Text;
using EventConnect.API.Middleware;
using EventConnect.Application.Services;
using EventConnect.Application.Services.Implementation;
using EventConnect.Domain.Repositories;
using EventConnect.Domain.Services;
using EventConnect.Infrastructure.Repositories;
using EventConnect.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS
var allowedOrigins = builder.Configuration.GetSection("AllowedCorsOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithMethods("GET", "POST", "PUT", "DELETE", "PATCH", "OPTIONS")
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With")
              .AllowCredentials();
    });
});

// Configure JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");
var key = Encoding.ASCII.GetBytes(secretKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure Rate Limiting
// TODO: Rate Limiting requiere investigación adicional para .NET 9.0
// La API de Rate Limiting puede haber cambiado en .NET 9.0
// Opciones: Usar middleware personalizado o paquete de terceros (ej: AspNetCoreRateLimit)
// var rateLimitConfig = builder.Configuration.GetSection("RateLimiting");
// if (rateLimitConfig.GetValue<bool>("EnableRateLimiting", true))
// {
//     // Implementar rate limiting aquí cuando se determine el approach correcto
// }

// Register repositories
var connectionString = builder.Configuration.GetConnectionString("EventConnectConnection") 
    ?? throw new InvalidOperationException("Connection string not found");

// Configure Health Checks (after connectionString is declared)
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString, name: "postgresql", tags: new[] { "db", "postgresql" });

// Core repositories
builder.Services.AddScoped<IUsuarioRepository>(_ => new UsuarioRepository(connectionString));
builder.Services.AddScoped<IContenidoLandingRepository>(_ => new ContenidoLandingRepository(connectionString));
builder.Services.AddScoped<IConfiguracionSistemaRepository>(_ => new ConfiguracionSistemaRepository(connectionString));

// TODO: Crear interfaces para los siguientes repositorios y registrar con DI
builder.Services.AddScoped(_ => new ProductoRepository(connectionString));
builder.Services.AddScoped(_ => new CategoriaRepository(connectionString));
builder.Services.AddScoped(_ => new ClienteRepository(connectionString));
builder.Services.AddScoped(_ => new ReservaRepository(connectionString));

// SIGI repositories
builder.Services.AddScoped(_ => new ActivoRepository(connectionString));
builder.Services.AddScoped(_ => new BodegaRepository(connectionString));
builder.Services.AddScoped(_ => new LoteRepository(connectionString));
builder.Services.AddScoped(_ => new MovimientoInventarioRepository(connectionString));
builder.Services.AddScoped(_ => new MantenimientoRepository(connectionString));

// Chat repositories
builder.Services.AddScoped(_ => new ConversacionRepository(connectionString));
builder.Services.AddScoped(_ => new MensajeRepository(connectionString));

// Payment repositories
builder.Services.AddScoped(_ => new TransaccionPagoRepository(connectionString));

// DetalleReserva repository
builder.Services.AddScoped(_ => new DetalleReservaRepository(connectionString));
builder.Services.AddScoped(_ => new DepreciacionRepository(connectionString));

// Catalog repositories
builder.Services.AddScoped(_ => new EstadoReservaRepository(connectionString));
builder.Services.AddScoped(_ => new EstadoActivoRepository(connectionString));
builder.Services.AddScoped(_ => new MetodoPagoRepository(connectionString));
builder.Services.AddScoped(_ => new TipoMantenimientoRepository(connectionString));

// Register services
builder.Services.AddScoped<IAuthService>(provider =>
    new AuthService(connectionString, builder.Configuration));

// Validation services
builder.Services.AddScoped<IDetalleReservaValidacionService, DetalleReservaValidacionService>();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "EventConnect API",
        Version = "v1",
        Description = "API para gestiÃ³n de eventos y mobiliario"
    });
    
    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Global Exception Handler (must be first)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Enable Swagger only in Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventConnect API v1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// TODO: Apply Rate Limiting when implemented
// if (rateLimitConfig.GetValue<bool>("EnableRateLimiting", true))
// {
//     app.UseRateLimiter();
// }

app.UseAuthentication();
app.UseAuthorization();

// Health Check endpoint
app.MapHealthChecks("/health");

app.MapControllers();

app.Run();
