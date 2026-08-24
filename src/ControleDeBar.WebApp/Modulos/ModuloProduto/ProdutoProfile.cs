using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloProduto;

namespace ControleDeBar.WebApp.Modulos.ModuloProduto;

public class ProdutoProfile : Profile
{
    public ProdutoProfile()
    {
        CreateMap<ListarProdutoDto, ListarProdutoViewModel>();

        CreateMap<CadastrarProdutoViewModel, CadastrarProdutoDto>();

        CreateMap<EditarProdutoViewModel, EditarProdutoDto>();

        CreateMap<DetalhesProdutoDto, EditarProdutoViewModel>();

        CreateMap<DetalhesProdutoDto, ExcluirProdutoViewModel>();
    }
}
