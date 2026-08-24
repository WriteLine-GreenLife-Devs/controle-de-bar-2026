using ControleDeBar.Aplicacao.Compartilhado;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using FluentResults;

namespace ControleDeBar.Aplicacao.Modulos.ModuloProduto;

public class ServicoProduto(
    IRepositorioProduto repositorioProduto,
    IRepositorioPedido repositorioPedido
) : ServicoBase<Produto>
{
    public Result Cadastrar(CadastrarProdutoDto dto)
    {
        if (ExisteProdutoComMesmoNome(dto.Nome))
            return Falha(nameof(dto.Nome), "Já existe um produto com este nome.");

        Produto novoProduto = new(dto.Nome, dto.Preco);

        Result resultadoValidacao = ValidarEntidade(novoProduto);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        repositorioProduto.Cadastrar(novoProduto);

        return Result.Ok();
    }

    public Result Editar(EditarProdutoDto dto)
    {
        if (ExisteProdutoComMesmoNome(dto.Nome, dto.Id))
            return Falha(nameof(dto.Nome), "Já existe um produto com este nome.");

        Produto produtoAtualizado = new(dto.Nome, dto.Preco);

        Result resultadoValidacao = ValidarEntidade(produtoAtualizado);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        bool conseguiuEditar = repositorioProduto.Editar(dto.Id, produtoAtualizado);

        if (!conseguiuEditar)
            return Falha(string.Empty, "Produto não encontrado.");

        return Result.Ok();
    }

    public Result Excluir(Guid id)
    {
        Produto? produto = repositorioProduto.SelecionarPorId(id);

        if (produto is null)
            return Falha(string.Empty, "Produto não encontrado.");

        bool produtoPossuiPedidos = repositorioPedido
            .SelecionarTodos()
            .Any(p => p.ProdutoId == id);

        if (produtoPossuiPedidos)
            return Falha(string.Empty, "Não é possível excluir este produto, pois ele está vinculado a um pedido.");

        repositorioProduto.Excluir(id);

        return Result.Ok();
    }

    public List<ListarProdutoDto> SelecionarTodos()
    {
        return repositorioProduto
            .SelecionarTodos()
            .Select(p => new ListarProdutoDto(p.Id, p.Nome, p.Preco))
            .ToList();
    }

    public List<ListarProdutoDto> Buscar(string? nomeProduto)
    {
        if (string.IsNullOrWhiteSpace(nomeProduto))
            return SelecionarTodos();

        return repositorioProduto
            .SelecionarTodos()
            .Where(p => p.Nome.Contains(nomeProduto.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(p => new ListarProdutoDto(p.Id, p.Nome, p.Preco))
            .ToList();
    }

    public Result<DetalhesProdutoDto> SelecionarPorId(Guid id)
    {
        Produto? produto = repositorioProduto.SelecionarPorId(id);

        if (produto is null)
            return Result.Fail("Produto não encontrado.");

        DetalhesProdutoDto dto = new(
            produto.Id,
            produto.Nome,
            produto.Preco
        );

        return Result.Ok(dto);
    }

    private bool ExisteProdutoComMesmoNome(string nome, Guid? idIgnorado = null)
    {
        return repositorioProduto
            .SelecionarTodos()
            .Any(p =>
                p.Id != idIgnorado &&
                p.Nome.Equals(nome.Trim(), StringComparison.OrdinalIgnoreCase)
            );
    }
}
