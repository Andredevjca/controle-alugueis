using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Repositories;
using SistemaAlugueis.Services;

var cultura = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Conta/Entrar";
        opcoes.LogoutPath = "/Conta/Sair";
        opcoes.AccessDeniedPath = "/Conta/Entrar";
        opcoes.Cookie.Name = "SistemaAlugueis.Auth";
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(opcoes =>
{
    opcoes.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddSingleton<ConexaoBanco>();
builder.Services.AddScoped<IRepositorioCasa, RepositorioCasa>();
builder.Services.AddScoped<IRepositorioInquilino, RepositorioInquilino>();
builder.Services.AddScoped<IRepositorioContrato, RepositorioContrato>();
builder.Services.AddScoped<IRepositorioFinanceiro, RepositorioFinanceiro>();
builder.Services.AddScoped<IRepositorioContaConsumo, RepositorioContaConsumo>();
builder.Services.AddScoped<IRepositorioObservacao, RepositorioObservacao>();
builder.Services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
builder.Services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();
builder.Services.AddScoped<IRepositorioConfiguracao, RepositorioConfiguracao>();
builder.Services.AddScoped<ServicoCasa>();
builder.Services.AddScoped<ServicoInquilino>();
builder.Services.AddScoped<ServicoContrato>();
builder.Services.AddScoped<ServicoFinanceiro>();
builder.Services.AddScoped<ServicoContaConsumo>();
builder.Services.AddScoped<ServicoObservacao>();
builder.Services.AddScoped<ServicoDashboard>();
builder.Services.AddScoped<ServicoRelatorio>();
builder.Services.AddScoped<ServicoAutenticacao>();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var autenticacao = escopo.ServiceProvider.GetRequiredService<ServicoAutenticacao>();
    await autenticacao.GarantirAdministradorAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = [cultura],
    SupportedUICultures = [cultura]
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
