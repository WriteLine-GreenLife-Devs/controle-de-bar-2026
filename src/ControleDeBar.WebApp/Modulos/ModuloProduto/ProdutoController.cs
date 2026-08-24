using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloProduto;
using ControleDeBar.WebApp.Compartilhado.Extensions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace ControleDeBar.WebApp.Modulos.ModuloProduto;

public class ProdutoController(
    ServicoProduto servicoProduto,
    IMapper mapeador
) : Controller
{
    [HttpGet]
    public ActionResult Listar(string? nome)
    {
        List<ListarProdutoDto> dtos = servicoProduto.Buscar(nome);

        List<ListarProdutoViewModel> listarVms =
            mapeador.Map<List<ListarProdutoViewModel>>(dtos);

        ViewBag.NomeBuscado = nome;

        return View(listarVms);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        return View(new CadastrarProdutoViewModel(string.Empty, 0));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Cadastrar(CadastrarProdutoViewModel cadastrarVm)
    {
        if (!ModelState.IsValid)
            return View(cadastrarVm);

        CadastrarProdutoDto dto =
            mapeador.Map<CadastrarProdutoDto>(cadastrarVm);

        Result resultado = servicoProduto.Cadastrar(dto);

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
        Result<DetalhesProdutoDto> resultado =
            servicoProduto.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);
            return RedirectToAction(nameof(Listar));
        }

        return View(
            mapeador.Map<EditarProdutoViewModel>(resultado.Value)
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Editar(EditarProdutoViewModel editarVm)
    {
        if (!ModelState.IsValid)
            return View(editarVm);

        EditarProdutoDto dto =
            mapeador.Map<EditarProdutoDto>(editarVm);

        Result resultado = servicoProduto.Editar(dto);

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
        Result<DetalhesProdutoDto> resultado =
            servicoProduto.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);
            return RedirectToAction(nameof(Listar));
        }

        return View(
            mapeador.Map<ExcluirProdutoViewModel>(resultado.Value)
        );
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Excluir(ExcluirProdutoViewModel excluirVm)
    {
        Result resultado =
            servicoProduto.Excluir(excluirVm.Id);

        if (resultado.IsFailed)
            TempData.AddErrorMessage(resultado);

        return RedirectToAction(nameof(Listar));
    }
}
