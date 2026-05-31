using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VShop.CartApi.Context;
using VShop.CartApi.DTOs.Mappings;
using VShop.CartApi.Repositories;

// ──────────────────────────────────────────────
//  Builder
// ──────────────────────────────────────────────
var builder = WebApplication.CreateBuilder(args);

// ── Controllers & API Explorer ────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// ── Swagger ───────────────────────────────────
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VShop.CartApi", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"'Bearer' [space] seu token",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                },
                Scheme = "oauth2",
                Name   = "Bearer",
                In     = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// ── Banco de Dados ────────────────────────────
var mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));


builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
builder.Services.AddScoped<ICartRepository, CartRepository>();


// ── CORS ──────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// ── Autenticação JWT (Identity Server)
//    A busca das chaves JWKS é protegida por try/catch para que o app
//    não quebre quando o Identity Server estiver offline
//    (ex.: durante dotnet ef migrations add).
IEnumerable<SecurityKey> signingKeys = Enumerable.Empty<SecurityKey>();

try
{
    var certHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    using var httpClient = new HttpClient(certHandler);

    var jwksJson = await httpClient.GetStringAsync(
        "https://localhost:7150/.well-known/openid-configuration/jwks");

    var jsonWebKeySet = new JsonWebKeySet(jwksJson);
    signingKeys = jsonWebKeySet.GetSigningKeys();
}
catch (Exception ex)
{
    Console.WriteLine($"[WARN] Identity Server indisponível — chaves JWT não carregadas: {ex.Message}");
}

builder.Services.AddAuthentication("Bearer")
       .AddJwtBearer("Bearer", options =>
       {
           options.Authority = "https://localhost:7150";
           options.RequireHttpsMetadata = false;

           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateAudience = true,
               ValidAudience = "vshop",
               IssuerSigningKeys = signingKeys,
               ValidateIssuerSigningKey = true,
           };

           options.BackchannelHttpHandler = new HttpClientHandler
           {
               ServerCertificateCustomValidationCallback =
                   HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
           };
       });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApiScope", policy =>
    {
        policy.RequireAuthenticatedUser();
        policy.RequireClaim("scope", "vshop");
    });
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();