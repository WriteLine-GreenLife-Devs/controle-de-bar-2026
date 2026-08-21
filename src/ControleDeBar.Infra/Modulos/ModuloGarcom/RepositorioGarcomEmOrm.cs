using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Infra.Compartilhado.Orm;

namespace ControleDeBar.Infra.Modulos.ModuloGarcom;

public sealed class RepositorioGarcomEmOrm(
    ControleDeBarDbContext dbContext
) : RepositorioBaseEmOrm<Garcom>(dbContext), IRepositorioGarcom;
