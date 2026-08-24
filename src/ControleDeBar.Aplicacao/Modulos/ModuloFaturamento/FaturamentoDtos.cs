namespace ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;

public record ContaFaturamentoDto(
    Guid ContaId,
    string NomeCliente,
    int NumeroMesa,
    DateTime DataFechamento,
    decimal Total
);

public record FaturamentoDiarioDto(
    DateTime Data,
    decimal Total,
    List<ContaFaturamentoDto> Contas
);
