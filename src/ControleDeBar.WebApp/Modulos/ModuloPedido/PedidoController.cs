using AutoMapper;
using ControleDeBar.Aplicacao.Modulos.ModuloPedido;
using ControleDeBar.Aplicacao.Modulos.ModuloProduto;
using ControleDeBar.WebApp.Compartilhado.Extensions;
using FluentResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleDeBar.WebApp.Modulos.ModuloPedido;

public class PedidoController(
    ServicoPedido servicoPedido,
    ServicoProduto servicoProduto,
    IMapper mapeador
) : Controller
{
    [HttpGet]
    public ActionResult Adicionar(Guid contaId)
    {
        AdicionarPedidoViewModel adicionarVm = new()
        {
            ContaId = contaId,
            Quantidade = 1
        };

        CarregarProdutos(adicionarVm);

        return View(adicionarVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Adicionar(AdicionarPedidoViewModel adicionarVm)
    {
        if (!ModelState.IsValid)
        {
            CarregarProdutos(adicionarVm);

            return View(adicionarVm);
        }

        AdicionarPedidoDto dto =
            mapeador.Map<AdicionarPedidoDto>(adicionarVm);

        Result resultado = servicoPedido.Adicionar(dto);

        if (resultado.IsFailed)
        {
            ModelState.AddModelError(resultado);

            CarregarProdutos(adicionarVm);

            return View(adicionarVm);
        }

        return RedirectToAction(
            "Detalhes",
            "Conta",
            new { id = adicionarVm.ContaId }
        );
    }

    [HttpGet]
    public ActionResult Remover(Guid id)
    {
        Result<ListarPedidoDto> resultado =
            servicoPedido.SelecionarPorId(id);

        if (resultado.IsFailed)
        {
            TempData.AddErrorMessage(resultado);

            return RedirectToAction("Listar", "Conta");
        }

        RemoverPedidoViewModel removerVm =
            mapeador.Map<RemoverPedidoViewModel>(resultado.Value);

        return View(removerVm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult Remover(RemoverPedidoViewModel removerVm)
    {
        Result resultado =
            servicoPedido.Remover(removerVm.Id);

        if (resultado.IsFailed)
            TempData.AddErrorMessage(resultado);

        return RedirectToAction(
            "Detalhes",
            "Conta",
            new { id = removerVm.ContaId }
        );
    }

    private void CarregarProdutos(AdicionarPedidoViewModel adicionarVm)
    {
        adicionarVm.Produtos = servicoProduto
            .SelecionarTodos()
            .Select(p => new SelectListItem(
                $"{p.Nome} - {p.Preco:C}",
                p.Id.ToString()
            ))
            .ToList();
    }
}
