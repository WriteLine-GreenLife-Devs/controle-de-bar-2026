using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Infra.Compartilhado.Orm;

namespace ControleDeBar.Infra.Modulos.ModuloConta;

public sealed class RepositorioContaEmOrm : RepositorioBaseEmOrm<Conta>, IRepositorioConta
{
    private readonly ControleDeBarDbContext dbContext;

    public RepositorioContaEmOrm(ControleDeBarDbContext dbContext)
        : base(dbContext)
    {
        this.dbContext = dbContext;
    }

    public void Salvar()
    {
        dbContext.SaveChanges();
    }
}
