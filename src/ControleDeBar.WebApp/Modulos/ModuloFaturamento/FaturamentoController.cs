using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloFaturamento;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeBar.WebApp.Modulos.ModuloFaturamento;

public class FaturamentoController(
    ServicoFaturamento servicoFaturamento,
    IMapper mapeador
) : Controller
{
    [HttpGet]
    public ActionResult Consultar(DateTime? data)
    {
        DateTime dataConsulta = data?.Date ?? DateTime.Today;
        FaturamentoDiarioDto faturamento = servicoFaturamento.Consultar(dataConsulta);
        ConsultarFaturamentoViewModel consultarVm =
            mapeador.Map<ConsultarFaturamentoViewModel>(faturamento);

        return View(consultarVm);
    }
}
