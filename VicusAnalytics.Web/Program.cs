using VicusAnalytics.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiBase = builder.Configuration["API_BASE_URL"]
    ?? Environment.GetEnvironmentVariable("API_BASE_URL")
    ?? "http://api:8080/";

builder.Services.AddHttpClient<ApiService>(c => c.BaseAddress = new Uri(apiBase));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<VicusAnalytics.Web.Components.App>()
    .AddInteractiveServerRenderMode();

app.Run();
