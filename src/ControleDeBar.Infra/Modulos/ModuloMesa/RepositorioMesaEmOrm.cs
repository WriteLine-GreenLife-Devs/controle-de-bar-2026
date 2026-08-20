using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Infra.Compartilhado.Orm;

namespace ControleDeBar.Infra.Modulos.ModuloMesa;

public sealed class RepositorioMesaEmOrm(
    ControleDeBarDbContext dbContext
) : RepositorioBaseEmOrm<Mesa>(dbContext), IRepositorioMesa;
