using System.ComponentModel.DataAnnotations;

namespace ControleDeBar.WebApp.Modulos.ModuloGarcom;

public record ListarGarcomViewModel(
    Guid Id,
    string Nome
);

public record CadastrarGarcomViewModel(
    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    string Nome
);

public record EditarGarcomViewModel(
    Guid Id,

    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    string Nome
);

public record ExcluirGarcomViewModel(
    Guid Id,
    string Nome
);
