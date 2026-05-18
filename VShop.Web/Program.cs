using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using VShop.Web.Services;
using VShop.Web.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

var certHandler = new HttpClientHandler
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
};

using var httpClient = new HttpClient(certHandler);
try
{
    var discovery = await httpClient.GetStringAsync(
        "https://localhost:7150/.well-known/openid-configuration");
    Console.WriteLine("✅ Discovery OK: " + discovery[..100]);
}
catch (Exception ex)
{
    Console.WriteLine("❌ Discovery FALHOU: " + ex.Message);
}

var metadataJson = await httpClient.GetStringAsync(
    "https://localhost:7150/.well-known/openid-configuration");
var metadata = System.Text.Json.JsonDocument.Parse(metadataJson).RootElement;

var jwksUri = metadata.GetProperty("jwks_uri").GetString()!;
var jwksJson = await httpClient.GetStringAsync(jwksUri);
var jsonWebKeySet = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(jwksJson);

var oidcConfig = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration
{
    Issuer = metadata.GetProperty("issuer").GetString(),
    AuthorizationEndpoint = metadata.GetProperty("authorization_endpoint").GetString(),
    TokenEndpoint = metadata.GetProperty("token_endpoint").GetString(),
    UserInfoEndpoint = metadata.GetProperty("userinfo_endpoint").GetString(),
    EndSessionEndpoint = metadata.GetProperty("end_session_endpoint").GetString(),
    JwksUri = jwksUri,
};

foreach (var key in jsonWebKeySet.GetSigningKeys())
    oidcConfig.SigningKeys.Add(key);

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Cookies";
    options.DefaultChallengeScheme = "oidc";
})
.AddCookie("Cookies", c =>
{
    c.ExpireTimeSpan = TimeSpan.FromMinutes(10);
    c.Events = new CookieAuthenticationEvents
    {
        OnRedirectToAccessDenied = context =>
        {
            context.Response.Redirect(
                builder.Configuration["ServiceUri:IdentityServer"] + "/Account/AccessDenied");
            return Task.CompletedTask;
        }
    };
})
.AddOpenIdConnect("oidc", options =>
{
    options.Authority = builder.Configuration["ServiceUri:IdentityServer"];
    options.ClientId = "vshop";
    options.ClientSecret = builder.Configuration["Client:Secret"];
    options.ResponseType = "code";
    options.ResponseMode = "query";
    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
    options.RequireHttpsMetadata = false;
    options.MapInboundClaims = false;
    options.UseTokenLifetime = false;

    options.Scope.Clear();
    options.Scope.Add("openid");
    options.Scope.Add("profile");
    options.Scope.Add("email");
    options.Scope.Add("vshop");

    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";
    options.ClaimActions.MapJsonKey("role", "role", "role");
    options.ClaimActions.MapJsonKey("sub", "sub", "sub");

    options.BackchannelHttpHandler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    options.Configuration = oidcConfig;

    options.Events = new OpenIdConnectEvents
    {
        OnAuthorizationCodeReceived = async context =>
        {
            var tokenClient = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            });

            var tokenRequest = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = context.TokenEndpointRequest!.Code,
                ["redirect_uri"] = context.TokenEndpointRequest.RedirectUri,
                ["client_id"] = "vshop",
                ["client_secret"] = builder.Configuration["Client:Secret"]!,
                ["code_verifier"] = context.TokenEndpointRequest.Parameters
                    .TryGetValue("code_verifier", out var cv) ? cv : ""
            };

            var response = await tokenClient.PostAsync(
                "https://localhost:7150/connect/token",
                new FormUrlEncodedContent(tokenRequest));

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine("🔍 RAW TOKEN RESPONSE: " + json[..50]);

            var tokenDoc = System.Text.Json.JsonDocument.Parse(json).RootElement;
            var accessToken = tokenDoc.GetProperty("access_token").GetString();
            var idToken = tokenDoc.GetProperty("id_token").GetString();

            context.HandleCodeRedemption(accessToken, idToken);
        },
        OnRemoteFailure = context =>
        {
            Console.WriteLine("🔥 ERRO OIDC: " + context.Failure?.Message);
            context.HandleResponse();
            context.Response.Redirect("/");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHttpClient("ProductApi", c =>
    c.BaseAddress = new Uri(builder.Configuration["ServiceUri:ProductsApi"]));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();