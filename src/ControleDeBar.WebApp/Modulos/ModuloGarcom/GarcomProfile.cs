using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloGarcom;

namespace ControleDeBar.WebApp.Modulos.ModuloGarcom;

public class GarcomProfile : Profile
{
    public GarcomProfile()
    {
        CreateMap<ListarGarcomDto, ListarGarcomViewModel>();
        CreateMap<CadastrarGarcomViewModel, CadastrarGarcomDto>();
        CreateMap<EditarGarcomViewModel, EditarGarcomDto>();
        CreateMap<DetalhesGarcomDto, EditarGarcomViewModel>();
        CreateMap<DetalhesGarcomDto, ExcluirGarcomViewModel>();
    }
}
