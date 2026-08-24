using ControleDeBar.Dominio.Compartilhado;
using ControleDeBar.Dominio.Compartilhado.Identity;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;

namespace ControleDeBar.Dominio.Modulos.ModuloConta;

public class Conta : EntidadeBase<Conta>, IEntidadeDoUsuario
{
    public Guid UserId { get; set; }

    public Guid MesaId { get; set; }
    public Mesa? Mesa { get; set; }

    public Guid GarcomId { get; set; }
    public Garcom? Garcom { get; set; }

    public string NomeCliente { get; set; } = string.Empty;

    public DateTime DataAbertura { get; set; }
    public DateTime? DataFechamento { get; set; }

    public StatusConta Status { get; set; }

    public Conta()
    {
    }

    public Conta(Guid mesaId, Guid garcomId, string nomeCliente) : this()
    {
        MesaId = mesaId;
        GarcomId = garcomId;
        NomeCliente = nomeCliente?.Trim() ?? string.Empty;

        DataAbertura = DateTime.Now;
        DataFechamento = null;
        Status = StatusConta.Aberta;
    }

    public override List<string> Validar()
    {
        List<string> erros = [];

        if (MesaId == Guid.Empty)
            erros.Add("O campo \"Mesa\" é obrigatório.");

        if (GarcomId == Guid.Empty)
            erros.Add("O campo \"Garçom\" é obrigatório.");

        if (string.IsNullOrWhiteSpace(NomeCliente))
            erros.Add("O campo \"Nome do cliente\" é obrigatório.");

        return erros;
    }

    public void Fechar()
    {
        if (Status == StatusConta.Fechada)
            return;

        Status = StatusConta.Fechada;
        DataFechamento = DateTime.Now;
    }

    public override void Atualizar(Conta entidadeAtualizada)
    {
        MesaId = entidadeAtualizada.MesaId;
        GarcomId = entidadeAtualizada.GarcomId;
        NomeCliente = entidadeAtualizada.NomeCliente?.Trim() ?? string.Empty;
    }
}
