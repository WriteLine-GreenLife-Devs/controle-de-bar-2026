using ControleDeBar.Dominio.Compartilhado;
using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloProduto;

namespace ControleDeBar.Dominio.Modulos.ModuloPedido;

public class Pedido : EntidadeBase<Pedido>, IEntidadeDoUsuario
{
    public Guid UserId { get; set; }

    public Guid ContaId { get; set; }
    public Conta? Conta { get; set; }

    public Guid ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    public string NomeProduto { get; set; } = string.Empty;
    public decimal PrecoPraticado { get; set; }

    public int Quantidade { get; set; }

    public decimal Subtotal => PrecoPraticado * Quantidade;

    public Pedido()
    {
    }

    public Pedido(
        Guid contaId,
        Guid produtoId,
        string nomeProduto,
        decimal precoPraticado,
        int quantidade
    ) : this()
    {
        ContaId = contaId;
        ProdutoId = produtoId;
        NomeProduto = nomeProduto?.Trim() ?? string.Empty;
        PrecoPraticado = precoPraticado;
        Quantidade = quantidade;
    }

    public override List<string> Validar()
    {
        List<string> erros = [];

        if (ContaId == Guid.Empty)
            erros.Add("O campo \"Conta\" é obrigatório.");

        if (ProdutoId == Guid.Empty)
            erros.Add("O campo \"Produto\" é obrigatório.");

        if (string.IsNullOrWhiteSpace(NomeProduto))
            erros.Add("O nome do produto é obrigatório.");

        if (PrecoPraticado <= 0)
            erros.Add("O preço praticado deve ser maior que zero.");

        if (Quantidade <= 0)
            erros.Add("O campo \"Quantidade\" deve ser maior que zero.");

        return erros;
    }

    public override void Atualizar(Pedido entidadeAtualizada)
    {
        ContaId = entidadeAtualizada.ContaId;
        ProdutoId = entidadeAtualizada.ProdutoId;
        NomeProduto = entidadeAtualizada.NomeProduto?.Trim() ?? string.Empty;
        PrecoPraticado = entidadeAtualizada.PrecoPraticado;
        Quantidade = entidadeAtualizada.Quantidade;
    }
}
