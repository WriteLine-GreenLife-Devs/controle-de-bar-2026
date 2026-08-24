using System.ComponentModel.DataAnnotations;

namespace ControleDeBar.WebApp.Modulos.ModuloProduto;

public record ListarProdutoViewModel(
    Guid Id,
    string Nome,
    decimal Preco
);

public record CadastrarProdutoViewModel(
    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    string Nome,

    [Range(typeof(decimal), "0,01", "99999999,99",
        ErrorMessage = "O campo \"Preço\" deve ser maior que zero.")]
    decimal Preco
);

public record EditarProdutoViewModel(
    Guid Id,

    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    string Nome,

    [Range(typeof(decimal), "0,01", "99999999,99",
        ErrorMessage = "O campo \"Preço\" deve ser maior que zero.")]
    decimal Preco
);

public record ExcluirProdutoViewModel(
    Guid Id,
    string Nome,
    decimal Preco
);
