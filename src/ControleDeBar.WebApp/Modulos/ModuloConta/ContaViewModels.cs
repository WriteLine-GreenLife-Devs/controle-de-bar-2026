using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.WebApp.Modulos.ModuloPedido;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleDeBar.WebApp.Modulos.ModuloConta;

public record ListarContaViewModel(
    Guid Id,
    string NomeCliente,
    int NumeroMesa,
    string NomeGarcom,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    StatusConta Status
);

public record ListagemContasViewModel(
    List<ListarContaViewModel> Abertas,
    List<ListarContaViewModel> Fechadas
);

public class AbrirContaViewModel
{
    [Required(ErrorMessage = "O campo \"Mesa\" é obrigatório.")]
    public Guid? MesaId { get; set; }

    [Required(ErrorMessage = "O campo \"Garçom\" é obrigatório.")]
    public Guid? GarcomId { get; set; }

    [Required(ErrorMessage = "O campo \"Nome do cliente\" é obrigatório.")]
    public string NomeCliente { get; set; } = string.Empty;

    public List<SelectListItem> Mesas { get; set; } = [];

    public List<SelectListItem> Garcons { get; set; } = [];
}

public record DetalhesContaViewModel(
    Guid Id,
    Guid MesaId,
    int NumeroMesa,
    Guid GarcomId,
    string NomeGarcom,
    string NomeCliente,
    DateTime DataAbertura,
    DateTime? DataFechamento,
    StatusConta Status,
    List<ListarPedidoViewModel> Pedidos,
    decimal Total
);

public record FecharContaViewModel(
    Guid Id,
    string NomeCliente,
    int NumeroMesa,
    string NomeGarcom,
    DateTime DataAbertura
);
