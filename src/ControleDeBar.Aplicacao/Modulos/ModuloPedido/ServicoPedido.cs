using ControleDeBar.Aplicacao.Compartilhado;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Dominio.Modulos.ModuloProduto;
using FluentResults;

namespace ControleDeBar.Aplicacao.Modulos.ModuloPedido;

public class ServicoPedido(
    IRepositorioPedido repositorioPedido,
    IRepositorioConta repositorioConta,
    IRepositorioProduto repositorioProduto
) : ServicoBase<Pedido>
{
    public Result Adicionar(AdicionarPedidoDto dto)
    {
        Conta? conta = repositorioConta.SelecionarPorId(dto.ContaId);

        if (conta is null)
            return Falha(nameof(dto.ContaId), "Conta não encontrada.");

        if (conta.Status == StatusConta.Fechada)
            return Falha(string.Empty, "Não é possível adicionar pedidos a uma conta fechada.");

        Produto? produto = repositorioProduto.SelecionarPorId(dto.ProdutoId);

        if (produto is null)
            return Falha(nameof(dto.ProdutoId), "Produto não encontrado.");

        Pedido novoPedido = new(
            conta.Id,
            produto.Id,
            produto.Nome,
            produto.Preco,
            dto.Quantidade
        );

        Result resultadoValidacao = ValidarEntidade(novoPedido);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        repositorioPedido.Cadastrar(novoPedido);

        return Result.Ok();
    }

    public Result Remover(Guid id)
    {
        Pedido? pedido = repositorioPedido.SelecionarPorId(id);

        if (pedido is null)
            return Falha(string.Empty, "Pedido não encontrado.");

        Conta? conta = repositorioConta.SelecionarPorId(pedido.ContaId);

        if (conta is null)
            return Falha(string.Empty, "Conta não encontrada.");

        if (conta.Status == StatusConta.Fechada)
            return Falha(
                string.Empty,
                "Não é possível remover pedidos de uma conta fechada."
            );

        repositorioPedido.Excluir(id);

        return Result.Ok();
    }

    public List<ListarPedidoDto> SelecionarPorConta(Guid contaId)
    {
        return repositorioPedido
            .SelecionarTodos()
            .Where(p => p.ContaId == contaId)
            .Select(MapearParaListagem)
            .ToList();
    }

    public decimal CalcularTotal(Guid contaId)
    {
        return repositorioPedido
            .SelecionarTodos()
            .Where(p => p.ContaId == contaId)
            .Sum(p => p.Subtotal);
    }

    public Result<ListarPedidoDto> SelecionarPorId(Guid id)
    {
        Pedido? pedido = repositorioPedido.SelecionarPorId(id);

        if (pedido is null)
            return Result.Fail("Pedido não encontrado.");

        return Result.Ok(MapearParaListagem(pedido));
    }

    private static ListarPedidoDto MapearParaListagem(Pedido pedido)
    {
        return new ListarPedidoDto(
            pedido.Id,
            pedido.ContaId,
            pedido.ProdutoId,
            pedido.NomeProduto,
            pedido.PrecoPraticado,
            pedido.Quantidade,
            pedido.Subtotal
        );
    }
}
