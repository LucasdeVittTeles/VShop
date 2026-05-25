using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;
using VShop.ProductApi;
using VShop.ProductApi.Context;
using VShop.ProductApi.DTOs.Mappings;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "VShop.ProductApi", Version = "v1" });
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
                  Id = "Bearer"
               },
               Scheme = "oauth2",
               Name = "Bearer",
               In = ParameterLocation.Header
            },
            new List<string>()
         }
    });
});

var mySqlConnection = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(opts =>
    opts.UseMySql(mySqlConnection, ServerVersion.AutoDetect(mySqlConnection)));

builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile));
builder.Services.AddCoreRepositoryDependencies();
builder.Services.AddCoreServicesDependencies();

// ? Carrega as chaves do Identity Server manualmente (resolve problema de certificado dev)
var certHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};
using var httpClient = new HttpClient(certHandler);
var jwksJson = await httpClient.GetStringAsync(
    "https://localhost:7150/.well-known/openid-configuration/jwks");
var jsonWebKeySet = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(jwksJson);
var signingKeys = jsonWebKeySet.GetSigningKeys();

builder.Services.AddAuthentication("Bearer")
       .AddJwtBearer("Bearer", options =>
       {
           options.Authority = "https://localhost:7150";
           options.RequireHttpsMetadata = false;
           options.TokenValidationParameters = new TokenValidationParameters
           {
               ValidateAudience = true,
               ValidAudience = "vshop",
               IssuerSigningKeys = signingKeys, // ? chaves injetadas manualmente
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
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();