namespace ControleDeBar.Aplicacao.Modulos.ModuloPedido;

public record AdicionarPedidoDto(
    Guid ContaId,
    Guid ProdutoId,
    int Quantidade
);

public record ListarPedidoDto(
    Guid Id,
    Guid ContaId,
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoPraticado,
    int Quantidade,
    decimal Subtotal
);
