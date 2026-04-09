using System.Security.Claims;
using System.Text;
using CarwashApi.Data;
using CarwashApi.Models;
using CarwashApi.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Npgsql.NameTranslation;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using CarwashApi;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options => 
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<AuthorizeSecurityOperationTransformer>();
    options.AddSchemaTransformer<SimplifyNumericOpenApiSchemaTransformer>();
}
);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict;
});

// Reuse a single translator instance so EF options stay stable.
var nameTranslator = new NpgsqlNullNameTranslator();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default"),
        npgsql =>
        {
            npgsql.MapEnum<DeliveryStatus>("delivery_status", "public", nameTranslator);
            npgsql.MapEnum<DevicePlatform>("device_platform", "public", nameTranslator);
        }));

// Services
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<IPushNotificationService, PushNotificationService>();

// JWT auth
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.RequireHttpsMetadata = false;

    var jwtSection = builder.Configuration.GetSection("Jwt");
    var issuer = jwtSection["Issuer"];
    var audience = jwtSection["Audience"];
    var secretKey = jwtSection["SecretKey"];

    if(string.IsNullOrEmpty(secretKey)) {
        throw new InvalidOperationException("Jwt SecretKey is missing in appsettings.json.");
    }

    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        RoleClaimType = ClaimTypes.Role,

        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.FromSeconds(30)
    };    
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
       policy.WithOrigins("http://localhost:3000", "http://localhost:5051")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Firebase Admin: uses GOOGLE_APPLICATION_CREDENTIALS (service account JSON path).
{
    var path = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS");
    if (string.IsNullOrWhiteSpace(path))
    {
        app.Logger.LogInformation(
            "Firebase Admin not initialized: set GOOGLE_APPLICATION_CREDENTIALS to your service account JSON path.");
    }
    else
    {
        if (!Path.IsPathRooted(path))
            path = Path.GetFullPath(Path.Combine(app.Environment.ContentRootPath, path));

        if (!File.Exists(path))
        {
            throw new InvalidOperationException(
                $"Firebase credentials file not found. GOOGLE_APPLICATION_CREDENTIALS resolved to: {path}");
        }

        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);

        if (FirebaseApp.DefaultInstance == null)
        {
            FirebaseApp.Create(new AppOptions { Credential = GoogleCredential.GetApplicationDefault() });
            app.Logger.LogInformation("Firebase Admin initialized.");
        }
    }
}

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "Carwash API v1");
});

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
