using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloPedido;

namespace ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;

public class ServicoFaturamento(
    IRepositorioConta repositorioConta,
    IRepositorioPedido repositorioPedido,
    IRepositorioMesa repositorioMesa
)
{
    public FaturamentoDiarioDto Consultar(DateTime data)
    {
        DateTime dataConsulta = data.Date;

        Dictionary<Guid, decimal> totaisPorConta = repositorioPedido
            .SelecionarTodos()
            .GroupBy(pedido => pedido.ContaId)
            .ToDictionary(
                grupo => grupo.Key,
                grupo => grupo.Sum(pedido => pedido.Subtotal)
            );

        Dictionary<Guid, Mesa> mesas = repositorioMesa
            .SelecionarTodos()
            .ToDictionary(mesa => mesa.Id);

        List<ContaFaturamentoDto> contas = repositorioConta
            .SelecionarTodos()
            .Where(conta =>
                conta.Status == StatusConta.Fechada &&
                conta.DataFechamento.HasValue &&
                conta.DataFechamento.Value.Date == dataConsulta
            )
            .Select(conta => new ContaFaturamentoDto(
                conta.Id,
                conta.NomeCliente,
                mesas.TryGetValue(conta.MesaId, out Mesa? mesa) ? mesa.Numero : 0,
                conta.DataFechamento!.Value,
                totaisPorConta.GetValueOrDefault(conta.Id)
            ))
            .ToList();

        return new FaturamentoDiarioDto(
            dataConsulta,
            contas.Sum(conta => conta.Total),
            contas
        );
    }
}
