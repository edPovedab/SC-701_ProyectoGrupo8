using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using PW.News8.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();


// ── Autenticación por cookies (propia del Web, independiente de la API) ──────
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.Cookie.Name = "PWNews8.Web.Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// (Login, AccessDenied, Error).
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

// ── HttpClient tipado hacia PW.News8.API ──────────────────────────────────────
// La URL base se lee de appsettings.json (ApiSettings:BaseUrl) para que el
// puerto de la API se pueda ajustar sin recompilar.
builder.Services.AddHttpClient<ISourceApiService, SourceApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException("Falta configurar ApiSettings:BaseUrl en appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
});
builder.Services.AddHttpClient<IConfigApiService, ConfigApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException("Falta configurar ApiSettings:BaseUrl en appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
});

// ── HttpClient tipado hacia PW.News8.API para el módulo de noticias (elemento sorpresa) ──
builder.Services.AddHttpClient<INewsClientService, NewsClientService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException("Falta configurar ApiSettings:BaseUrl en appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
});

// ── HttpClient tipado hacia PW.News8.API para autenticación ──────────────────
builder.Services.AddHttpClient<IAuthApiService, AuthApiService>(client =>
{
    var baseUrl = builder.Configuration["ApiSettings:BaseUrl"]
                  ?? throw new InvalidOperationException("Falta configurar ApiSettings:BaseUrl en appsettings.json.");
    client.BaseAddress = new Uri(baseUrl);
});

var app = builder.Build();

// configuracion del middleware de la aplicación para manejar errores y seguridad en producción
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets().AllowAnonymous();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();