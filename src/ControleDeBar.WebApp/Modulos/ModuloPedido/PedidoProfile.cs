using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloPedido;

namespace ControleDeBar.WebApp.Modulos.ModuloPedido;

public class PedidoProfile : Profile
{
    public PedidoProfile()
    {
        CreateMap<AdicionarPedidoViewModel, AdicionarPedidoDto>();
        CreateMap<ListarPedidoDto, RemoverPedidoViewModel>();
        CreateMap<ListarPedidoDto, ListarPedidoViewModel>();
    }
}
