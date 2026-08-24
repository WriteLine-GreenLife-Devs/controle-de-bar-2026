using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloConta;

namespace ControleDeBar.WebApp.Modulos.ModuloConta;

public class ContaProfile : Profile
{
    public ContaProfile()
    {
        CreateMap<ListarContaDto, ListarContaViewModel>();

        CreateMap<AbrirContaViewModel, AbrirContaDto>();

        CreateMap<DetalhesContaDto, DetalhesContaViewModel>();

        CreateMap<DetalhesContaDto, FecharContaViewModel>();
    }
}
