using Grpc.Net.Client;
using Microsoft.AspNetCore.Components.Server.Circuits;
using Microsoft.AspNetCore.DataProtection;
using Pocco.Client.Web;
using Pocco.Client.Web.Clients;
using Pocco.Client.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataProtection();

builder.Services.AddSingleton(sp =>
{
    var connectionString = Environment.GetEnvironmentVariable("API_CONNECTION_URL") ?? throw new InvalidOperationException("API_CONNECTION_URL is not set in environment variables");
    var grpcChannel = GrpcChannel.ForAddress(connectionString, new GrpcChannelOptions
    {
        HttpHandler = new SocketsHttpHandler
        {
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(5),
            KeepAlivePingDelay = TimeSpan.FromMinutes(1),
            KeepAlivePingTimeout = TimeSpan.FromSeconds(20),
            EnableMultipleHttp2Connections = true
        }
    });
    return grpcChannel;
});
builder.Services.AddScoped<AuthClient>();
builder.Services.AddSingleton<GrpcClientFeederProvider>();
builder.Services.AddScoped<CircuitHandler, CircuitClosureDetector>();

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
