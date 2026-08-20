using System.ComponentModel.DataAnnotations;
using ControleDeBar.Dominio.Modulos.ModuloMesa;

namespace ControleDeBar.WebApp.Modulos.ModuloMesa;

public record ListarMesaViewModel(
    Guid Id,
    int Numero,
    int Lugares,
    StatusMesa Status
);

public record CadastrarMesaViewModel(
    [Range(1, int.MaxValue, ErrorMessage = "O campo \"Número\" deve ser maior que zero.")]
    int Numero,

    [Range(1, int.MaxValue, ErrorMessage = "O campo \"Lugares\" deve ser maior que zero.")]
    int Lugares
);

public record EditarMesaViewModel(
    Guid Id,

    [Range(1, int.MaxValue, ErrorMessage = "O campo \"Número\" deve ser maior que zero.")]
    int Numero,

    [Range(1, int.MaxValue, ErrorMessage = "O campo \"Lugares\" deve ser maior que zero.")]
    int Lugares
);

public record ExcluirMesaViewModel(
    Guid Id,
    int Numero,
    int Lugares
);
