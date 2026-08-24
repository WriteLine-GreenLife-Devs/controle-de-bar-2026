using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloConta;
using ControleDeBar.Aplicacao.Modulos.ModuloGarcom;
using ControleDeBar.Aplicacao.Modulos.ModuloMesa;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.WebApp.Compartilhado.Extensions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleDeBar.WebApp.Modulos.ModuloConta;

public class ContaController(
    ServicoConta servicoConta,
    ServicoMesa servicoMesa,
    ServicoGarcom servicoGarcom,
    IMapper mapeador
) : Controller
{
    [HttpGet]
    public ActionResult Listar()
    {
        List<ListarContaDto> abertas =
            servicoConta.SelecionarAbertas();

        List<ListarContaDto> fechadas =
            servicoConta.SelecionarFechadas();

        ListagemContasViewModel listarVm = new(
            mapeador.Map<List<ListarContaViewModel>>(abertas),
            mapeador.Map<List<ListarContaViewModel>>(fechadas)
        );

        return View(listarVm);
    }

    [HttpGet]
    public ActionResult Abrir()
    {
        AbrirContaViewModel abrirVm = new();

        CarregarOpcoes(abrirVm);

        return View(abrirVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Abrir(AbrirContaViewModel abrirVm)
    {
        if (!ModelState.IsValid)
        {
            CarregarOpcoes(abrirVm);

            return View(abrirVm);
        }

        AbrirContaDto dto =
            mapeador.Map<AbrirContaDto>(abrirVm);

        Result resultado = servicoConta.Abrir(dto);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError(resultado);

            CarregarOpcoes(abrirVm);

            return View(abrirVm);
        }

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Detalhes(Guid id)
    {
        Result<DetalhesContaDto> resultado =
            servicoConta.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);

            return RedirectToAction(nameof(Listar));
        }

        DetalhesContaViewModel detalhesVm =
            mapeador.Map<DetalhesContaViewModel>(resultado.Value);

        return View(detalhesVm);
    }

    [HttpGet]
    public ActionResult Fechar(Guid id)
    {
        Result<DetalhesContaDto> resultado =
            servicoConta.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);

            return RedirectToAction(nameof(Listar));
        }

        if (resultado.Value.Status == StatusConta.Fechada)
        {
            TempData["MensagemErro"] = "A conta já está fechada.";

            return RedirectToAction(nameof(Listar));
        }

        FecharContaViewModel fecharVm =
            mapeador.Map<FecharContaViewModel>(resultado.Value);

        return View(fecharVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Fechar(FecharContaViewModel fecharVm)
    {
        Result resultado =
            servicoConta.Fechar(fecharVm.Id);

        if (resultado.IsFailed)
            TempData.AddErrorMessage(resultado);

        return RedirectToAction(nameof(Listar));
    }

    private void CarregarOpcoes(AbrirContaViewModel abrirVm)
    {
        abrirVm.Mesas = servicoMesa
            .SelecionarTodos()
            .Select(m => new SelectListItem(
                $"Mesa {m.Numero} - {m.Lugares} lugares",
                m.Id.ToString()
            ))
            .ToList();

        abrirVm.Garcons = servicoGarcom
            .SelecionarTodos()
            .Select(g => new SelectListItem(
                g.Nome,
                g.Id.ToString()
            ))
            .ToList();
    }
}
