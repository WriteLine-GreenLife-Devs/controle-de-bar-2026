using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleDeBar.WebApp.Modulos.ModuloPedido;

public class AdicionarPedidoViewModel
{
    public Guid ContaId { get; set; }

    [Required(ErrorMessage = "O campo \"Produto\" é obrigatório.")]
    public Guid? ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "O campo \"Quantidade\" deve ser maior que zero.")]
    public int Quantidade { get; set; }

    public List<SelectListItem> Produtos { get; set; } = [];
}

public record ListarPedidoViewModel(
    Guid Id,
    Guid ContaId,
    Guid ProdutoId,
    string NomeProduto,
    decimal PrecoPraticado,
    int Quantidade,
    decimal Subtotal
);
public record RemoverPedidoViewModel(
    Guid Id,
    Guid ContaId,
    string NomeProduto,
    decimal PrecoPraticado,
    int Quantidade,
    decimal Subtotal
);
