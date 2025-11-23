using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Pocco.APIClient.Core;
using Pocco.Client.Web;
using Pocco.Client.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddDataProtection();
builder.Services.AddScoped<ProtectedLocalStorageProvider>();

// API Clients
builder.Services.AddSingleton(sp =>
{
    return new APIClientConfigurations(APIClientType.User);
});
builder.Services.AddScoped(sp =>
{
    var config = sp.GetRequiredService<APIClientConfigurations>();
    var logger = sp.GetRequiredService<ILogger<APIClient>>();
    return new APIClient(config, logger);
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    const string CookieName = ".Pocco.Client.Web.DataProtection";
    if (!context.Request.Cookies.ContainsKey(CookieName))
    {
        // Random bytes
        var randomBytes = new byte[32];
        Random.Shared.NextBytes(randomBytes);
        var cookieValue = Convert.ToBase64String(randomBytes);

        // Encrypt cookie value
        var protector = app.Services.GetRequiredService<IDataProtectionProvider>()
            .CreateProtector("Pocco.Client.Web.CookieProtection");
        var protectedValue = protector.Protect(cookieValue);

        // TODO: Store to cookie to redis
        // TTL short: 15 minutes
        context.Response.Cookies.Append(CookieName, protectedValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/",
            MaxAge = TimeSpan.FromMinutes(15)
        });
    }

    await next();
});

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
