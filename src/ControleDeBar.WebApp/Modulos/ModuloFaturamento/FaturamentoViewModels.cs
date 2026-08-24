using System.ComponentModel.DataAnnotations;

namespace ControleDeBar.WebApp.Modulos.ModuloFaturamento;

public record ContaFaturamentoViewModel(
    Guid ContaId,
    string NomeCliente,
    int NumeroMesa,
    DateTime DataFechamento,
    decimal Total
);

public class ConsultarFaturamentoViewModel
{
    [DataType(DataType.Date)]
    public DateTime Data { get; set; }

    public decimal Total { get; set; }

    public List<ContaFaturamentoViewModel> Contas { get; set; } = [];
}
