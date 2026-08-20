using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloMesa;

namespace ControleDeBar.WebApp.Modulos.ModuloMesa;

public class MesaProfile : Profile
{
    public MesaProfile()
    {
        CreateMap<ListarMesaDto, ListarMesaViewModel>();
        CreateMap<CadastrarMesaViewModel, CadastrarMesaDto>();
        CreateMap<EditarMesaViewModel, EditarMesaDto>();
        CreateMap<DetalhesMesaDto, EditarMesaViewModel>();
        CreateMap<DetalhesMesaDto, ExcluirMesaViewModel>();
    }
}
