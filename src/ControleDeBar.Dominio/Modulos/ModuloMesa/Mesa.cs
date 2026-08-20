using ControleDeBar.Dominio.Compartilhado;
using ControleDeBar.Dominio.Compartilhado.Identity;

namespace ControleDeBar.Dominio.Modulos.ModuloMesa;

public class Mesa : EntidadeBase<Mesa>, IEntidadeDoUsuario
{
    public Guid UserId { get; set; }
    public int Numero { get; set; }
    public int Lugares { get; set; }
    public StatusMesa Status { get; set; }

    public Mesa()
    {
    }

    public Mesa(int numero, int lugares) : this()
    {
        Numero = numero;
        Lugares = lugares;
        Status = StatusMesa.Livre;
    }

    public override List<string> Validar()
    {
        List<string> erros = [];

        if (Numero <= 0)
            erros.Add("O campo \"Número\" deve ser maior que zero.");

        if (Lugares <= 0)
            erros.Add("O campo \"Lugares\" deve ser maior que zero.");

        if (Status == StatusMesa.Indeterminado)
            erros.Add("O campo \"Status\" deve ser informado.");

        return erros;
    }

    public override void Atualizar(Mesa entidadeAtualizada)
    {
        Numero = entidadeAtualizada.Numero;
        Lugares = entidadeAtualizada.Lugares;
    }

    public void Ocupar()
    {
        Status = StatusMesa.Ocupada;
    }

    public void Liberar()
    {
        Status = StatusMesa.Livre;
    }
}
