using SistemaAlugueis.Interfaces.Repositories;
using SistemaAlugueis.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using SistemaAlugueis.Infraestrutura;
using SistemaAlugueis.Repositories;
using SistemaAlugueis.Services;

namespace SistemaAlugueis.Dependecias;

public static class InjecaoDependencias
{
    public static IServiceCollection AdicionarDependencias(this IServiceCollection services)
    {
        services.AddSingleton<ConexaoBanco>();
        services.AddScoped<IRepositorioCasa, RepositorioCasa>();
        services.AddScoped<IRepositorioInquilino, RepositorioInquilino>();
        services.AddScoped<IRepositorioContrato, RepositorioContrato>();
        services.AddScoped<IRepositorioFinanceiro, RepositorioFinanceiro>();
        services.AddScoped<IRepositorioContaConsumo, RepositorioContaConsumo>();
        services.AddScoped<IRepositorioObservacao, RepositorioObservacao>();
        services.AddScoped<IRepositorioUsuario, RepositorioUsuario>();
        services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();
        services.AddScoped<IRepositorioConfiguracao, RepositorioConfiguracao>();
        services.AddScoped<IServicoCasa, ServicoCasa>();
        services.AddScoped<IServicoInquilino, ServicoInquilino>();
        services.AddScoped<IServicoContrato, ServicoContrato>();
        services.AddScoped<IServicoFinanceiro, ServicoFinanceiro>();
        services.AddScoped<IServicoContaConsumo, ServicoContaConsumo>();
        services.AddScoped<IServicoObservacao, ServicoObservacao>();
        services.AddScoped<IServicoDashboard, ServicoDashboard>();
        services.AddScoped<IServicoRelatorio, ServicoRelatorio>();
        services.AddScoped<IServicoAutenticacao, ServicoAutenticacao>();

        services.AddScoped<IServicoCategoria, ServicoCategoria>();
        services.AddScoped<IServicoConfiguracao, ServicoConfiguracao>();
        services.AddScoped<IServicoUsuario, ServicoUsuario>();
        return services;
    }
}