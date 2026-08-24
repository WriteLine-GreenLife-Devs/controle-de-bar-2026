using ControleDeBar.Dominio.Compartilhado;
using ControleDeBar.Dominio.Compartilhado.Identity;

namespace ControleDeBar.Dominio.Modulos.ModuloProduto;

public class Produto : EntidadeBase<Produto>, IEntidadeDoUsuario
{
    public Guid UserId { get; set; }
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }

    public Produto()
    {
    }

    public Produto(string nome, decimal preco) : this()
    {
        Nome = nome?.Trim() ?? string.Empty;
        Preco = preco;
    }

    public override List<string> Validar()
    {
        List<string> erros = [];

        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add("O campo \"Nome\" é obrigatório.");

        if (Preco <= 0)
            erros.Add("O campo \"Preço\" deve ser maior que zero.");

        return erros;
    }

    public override void Atualizar(Produto entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome?.Trim() ?? string.Empty;
        Preco = entidadeAtualizada.Preco;
    }
}
