using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControleDeBar.Aplicacao.Modulos.ModuloMesa;

namespace ControleDeBar.Aplicacao;

public static class InjecaoDeDependencia
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<ServicoMesa>();
    }
}
