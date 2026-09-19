using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using SistemaAlugueis.Dependecias;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Interfaces.Services;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;

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

builder.Services.AdicionarDependencias();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var atualizador = escopo.ServiceProvider.GetRequiredService<AtualizadorBanco>();
    await atualizador.AtualizarAsync();

    var autenticacao = escopo.ServiceProvider.GetRequiredService<IServicoAutenticacao>();
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

// Abre o navegador somente depois que o servidor estiver pronto.
if (OperatingSystem.IsWindows() && Environment.UserInteractive &&
    app.Configuration.GetValue("AbrirNavegadorAoIniciar", true))
{
    app.Lifetime.ApplicationStarted.Register(() =>
    {
        _ = Task.Run(() =>
        {
            try
            {
                var endereco = app.Urls.FirstOrDefault(url =>
                    url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
                    ?? app.Urls.FirstOrDefault();

                if (string.IsNullOrWhiteSpace(endereco))
                    return;

                endereco = endereco.Replace("://+:", "://localhost:")
                    .Replace("://*:", "://localhost:");
                var uri = new UriBuilder(endereco);
                if (uri.Host is "0.0.0.0" or "::" or "[::]")
                    uri.Host = "localhost";

                Process.Start(new ProcessStartInfo(uri.Uri.AbsoluteUri)
                {
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex,
                    "Nao foi possivel abrir o navegador automaticamente. Acesse a URL exibida no console.");
            }
        });
    });
}

app.Run();
