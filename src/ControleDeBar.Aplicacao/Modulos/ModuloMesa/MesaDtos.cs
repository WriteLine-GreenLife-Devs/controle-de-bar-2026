using ControleDeBar.Dominio.Modulos.ModuloMesa;

namespace ControleDeBar.Aplicacao.Modulos.ModuloMesa;

public record ListarMesaDto(
    Guid Id,
    int Numero,
    int Lugares,
    StatusMesa Status
);

public record CadastrarMesaDto(
    int Numero,
    int Lugares
);

public record EditarMesaDto(
    Guid Id,
    int Numero,
    int Lugares
);

public record DetalhesMesaDto(
    Guid Id,
    int Numero,
    int Lugares,
    StatusMesa Status
);
