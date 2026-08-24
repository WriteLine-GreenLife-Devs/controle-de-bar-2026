using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;

namespace ControleDeBar.WebApp.Modulos.ModuloFaturamento;

public class FaturamentoProfile : Profile
{
    public FaturamentoProfile()
    {
        CreateMap<ContaFaturamentoDto, ContaFaturamentoViewModel>();
        CreateMap<FaturamentoDiarioDto, ConsultarFaturamentoViewModel>();
    }
}
