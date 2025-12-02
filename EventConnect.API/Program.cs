using System.Text;
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
              .AllowAnyMethod()
              .AllowAnyHeader()
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
    options.RequireHttpsMetadata = false;
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

// Register repositories
var connectionString = builder.Configuration.GetConnectionString("EventConnectConnection") 
    ?? throw new InvalidOperationException("Connection string not found");

// Core repositories
builder.Services.AddScoped<IUsuarioRepository>(_ => new UsuarioRepository(connectionString));
builder.Services.AddScoped(_ => new UsuarioRepository(connectionString));
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

// Enable Swagger in all environments (for testing)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EventConnect API v1");
    c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
