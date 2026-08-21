using ControleDeBar.Dominio.Compartilhado;
using ControleDeBar.Dominio.Compartilhado.Identity;

namespace ControleDeBar.Dominio.Modulos.ModuloGarcom;

public class Garcom : EntidadeBase<Garcom>, IEntidadeDoUsuario
{
    public Guid UserId { get; set; }
    public string Nome { get; set; } = string.Empty;

    public Garcom()
    {
    }

    public Garcom(string nome) : this()
    {
        Nome = nome?.Trim() ?? string.Empty;
    }

    public override List<string> Validar()
    {
        List<string> erros = [];

        if (string.IsNullOrWhiteSpace(Nome))
            erros.Add("O campo \"Nome\" é obrigatório.");

        return erros;
    }

    public override void Atualizar(Garcom entidadeAtualizada)
    {
        Nome = entidadeAtualizada.Nome;
    }
}
