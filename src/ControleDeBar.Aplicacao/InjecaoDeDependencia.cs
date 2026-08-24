using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ControleDeBar.Aplicacao.Modulos.ModuloGarcom;
using ControleDeBar.Aplicacao.Modulos.ModuloMesa;
using ControleDeBar.Aplicacao.Modulos.ModuloProduto;
using ControleDeBar.Aplicacao.Modulos.ModuloConta;
using ControleDeBar.Aplicacao.Modulos.ModuloPedido;
using ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;

namespace ControleDeBar.Aplicacao;

public static class InjecaoDeDependencia
{
    public static void AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddScoped<ServicoGarcom>();
        services.AddScoped<ServicoMesa>();
        services.AddScoped<ServicoProduto>();
        services.AddScoped<ServicoConta>();
        services.AddScoped<ServicoPedido>();
        services.AddScoped<ServicoFaturamento>();
    }
}
