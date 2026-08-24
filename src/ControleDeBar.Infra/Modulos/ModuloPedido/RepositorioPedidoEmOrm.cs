using ControleDeBar.Dominio.Modulos.ModuloPedido;
using ControleDeBar.Infra.Compartilhado.Orm;

namespace ControleDeBar.Infra.Modulos.ModuloPedido;

public sealed class RepositorioPedidoEmOrm(
    ControleDeBarDbContext dbContext
) : RepositorioBaseEmOrm<Pedido>(dbContext), IRepositorioPedido;
