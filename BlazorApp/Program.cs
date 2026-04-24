using BlazorApp.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddSteam(options =>
{
    options.ApplicationKey = builder.Configuration["SteamApi:Key"];
});
builder.Services.AddScoped<RawgService>();
builder.Services.AddHttpClient("Rawg", client =>
{
    client.BaseAddress = new Uri("https://api.rawg.io/api/");
});

builder.Services.AddHttpClient("Steam", client =>
{
    client.BaseAddress = new Uri("https://api.steampowered.com/");
});

builder.Services.AddHttpClient("SteamStore", client =>
{
    client.BaseAddress = new Uri("https://store.steampowered.com/api/");
});

builder.Services.AddScoped<SteamService>();
builder.Services.AddScoped<MashupService>();
builder.Services.AddCascadingAuthenticationState();

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

app.MapGet("/auth/login", () =>
    Results.Challenge(
        new AuthenticationProperties { RedirectUri = "/" },
        new[] { "Steam" }
    ));

app.MapGet("/auth/logout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/my-games");
});

app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
