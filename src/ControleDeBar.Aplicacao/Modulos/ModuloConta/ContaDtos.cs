using ControleDeBar.Dominio.Modulos.ModuloConta;

namespace ControleDeBar.Aplicacao.Modulos.ModuloConta;

public record ListarContaDto(
    Guid Id,
    string NomeCliente,
    int NumeroMesa,
    string NomeGarcom,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    StatusConta Status
);

public record AbrirContaDto(
    Guid MesaId,
    Guid GarcomId,
    string NomeCliente
);

public record DetalhesContaDto(
    Guid Id,
    Guid MesaId,
    int NumeroMesa,
    Guid GarcomId,
    string NomeGarcom,
    string NomeCliente,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    StatusConta Status
);
