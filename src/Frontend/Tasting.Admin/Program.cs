using Tasting.Admin.Components;
using Tasting.Admin.Features.Auth.Services;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddMudServices();

builder.Services.AddScoped<TastingAuthStateProvider>();
builder.Services.AddScoped<IAuthApiClient, AuthApiClient>();
builder.Services.AddScoped<Tasting.Admin.Features.Identity.Services.IUsersApiClient, Tasting.Admin.Features.Identity.Services.UsersApiClient>();
builder.Services.AddScoped<Tasting.Admin.Features.Arrangement.Services.IArrangementsApiClient, Tasting.Admin.Features.Arrangement.Services.ArrangementsApiClient>();
builder.Services.AddScoped<Tasting.Admin.Features.Catalog.Services.IBreweriesApiClient, Tasting.Admin.Features.Catalog.Services.BreweriesApiClient>();
builder.Services.AddScoped<Tasting.Admin.Features.Catalog.Services.IBeersApiClient, Tasting.Admin.Features.Catalog.Services.BeersApiClient>();
builder.Services.AddHttpClient("Tasting.Api", client =>
{
    var baseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://localhost:7100/";
    client.BaseAddress = new Uri(baseUrl);
}).AddServiceDiscovery();
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Tasting.Api"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

// Needed for _content/ and _framework/ files in development/when not pre-built
app.UseStaticFiles();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.Run();
