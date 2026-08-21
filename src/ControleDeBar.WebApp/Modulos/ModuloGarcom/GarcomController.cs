using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloGarcom;
using ControleDeBar.WebApp.Compartilhado.Extensions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeBar.WebApp.Modulos.ModuloGarcom;

public class GarcomController(
    ServicoGarcom servicoGarcom,
    IMapper mapeador
) : Controller
{
    [HttpGet]
    public ActionResult Listar()
    {
        List<ListarGarcomDto> dtos = servicoGarcom.SelecionarTodos();

        List<ListarGarcomViewModel> listarVms = mapeador.Map<List<ListarGarcomViewModel>>(dtos);

        return View(listarVms);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        return View(new CadastrarGarcomViewModel(string.Empty));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Cadastrar(CadastrarGarcomViewModel cadastrarVm)
    {
        if (!ModelState.IsValid)
            return View(cadastrarVm);

        CadastrarGarcomDto dto = mapeador.Map<CadastrarGarcomDto>(cadastrarVm);
        Result resultado = servicoGarcom.Cadastrar(dto);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError(resultado);
            return View(cadastrarVm);
        }

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Editar(Guid id)
    {
        Result<DetalhesGarcomDto> resultado = servicoGarcom.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);
            return RedirectToAction(nameof(Listar));
        }

        return View(mapeador.Map<EditarGarcomViewModel>(resultado.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Editar(EditarGarcomViewModel editarVm)
    {
        if (!ModelState.IsValid)
            return View(editarVm);

        EditarGarcomDto dto = mapeador.Map<EditarGarcomDto>(editarVm);
        Result resultado = servicoGarcom.Editar(dto);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError(resultado);
            return View(editarVm);
        }

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Excluir(Guid id)
    {
        Result<DetalhesGarcomDto> resultado = servicoGarcom.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);
            return RedirectToAction(nameof(Listar));
        }

        return View(mapeador.Map<ExcluirGarcomViewModel>(resultado.Value));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Excluir(ExcluirGarcomViewModel excluirVm)
    {
        Result resultado = servicoGarcom.Excluir(excluirVm.Id);

        if (resultado.IsFailed)
            TempData.AddErrorMessage(resultado);

        return RedirectToAction(nameof(Listar));
    }
}
