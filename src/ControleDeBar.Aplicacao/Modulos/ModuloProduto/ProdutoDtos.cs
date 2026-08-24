namespace ControleDeBar.Aplicacao.Modulos.ModuloProduto;

public record ListarProdutoDto(
    Guid Id,
    string Nome,
    decimal Preco
);

public record CadastrarProdutoDto(
    string Nome,
    decimal Preco
);

public record EditarProdutoDto(
    Guid Id,
    string Nome,
    decimal Preco
);

public record DetalhesProdutoDto(
    Guid Id,
    string Nome,
    decimal Preco
);
