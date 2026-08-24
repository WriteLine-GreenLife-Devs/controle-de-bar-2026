using ControleDeBar.Dominio.Compartilhado;

namespace ControleDeBar.Dominio.Modulos.ModuloConta;

public interface IRepositorioConta : IRepositorio<Conta>
{
    void Salvar();
}
