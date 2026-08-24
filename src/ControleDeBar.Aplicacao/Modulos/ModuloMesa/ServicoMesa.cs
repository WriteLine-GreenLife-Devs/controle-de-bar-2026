using ControleDeBar.Aplicacao.Compartilhado;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using FluentResults;

namespace ControleDeBar.Aplicacao.Modulos.ModuloMesa;

public class ServicoMesa(
    IRepositorioMesa repositorioMesa,
    IRepositorioConta repositorioConta
) : ServicoBase<Mesa>
{
    public Result Cadastrar(CadastrarMesaDto dto)
    {
        if (ExisteMesaComMesmoNumero(dto.Numero))
            return Falha(nameof(dto.Numero), "Já existe uma mesa com este número.");

        Mesa novaMesa = new(dto.Numero, dto.Lugares);

        Result resultadoValidacao = ValidarEntidade(novaMesa);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        repositorioMesa.Cadastrar(novaMesa);

        return Result.Ok();
    }

    public Result Editar(EditarMesaDto dto)
    {
        if (ExisteMesaComMesmoNumero(dto.Numero, dto.Id))
            return Falha(nameof(dto.Numero), "Já existe uma mesa com este número.");

        Mesa mesaAtualizada = new(dto.Numero, dto.Lugares);

        Result resultadoValidacao = ValidarEntidade(mesaAtualizada);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        bool conseguiuEditar = repositorioMesa.Editar(dto.Id, mesaAtualizada);

        if (!conseguiuEditar)
            return Falha(string.Empty, "Mesa não encontrada.");

        return Result.Ok();
    }

    public Result Excluir(Guid id)
    {
        Mesa? mesa = repositorioMesa.SelecionarPorId(id);

        if (mesa is null)
            return Falha(string.Empty, "Mesa não encontrada.");

        bool mesaPossuiContas = repositorioConta
            .SelecionarTodos()
            .Any(conta => conta.MesaId == id);

        if (mesaPossuiContas)
            return Falha(
                string.Empty,
                "Não é possível excluir esta mesa, pois ela está vinculada a uma conta."
            );

        repositorioMesa.Excluir(id);

        return Result.Ok();
    }

    public List<ListarMesaDto> SelecionarTodos()
    {
        return repositorioMesa
            .SelecionarTodos()
            .Select(m => new ListarMesaDto(m.Id, m.Numero, m.Lugares, m.Status))
            .ToList();
    }

    public Result<DetalhesMesaDto> SelecionarPorId(Guid id)
    {
        Mesa? mesa = repositorioMesa.SelecionarPorId(id);

        if (mesa is null)
            return Result.Fail("Mesa não encontrada.");

        DetalhesMesaDto dto = new(mesa.Id, mesa.Numero, mesa.Lugares, mesa.Status);

        return Result.Ok(dto);
    }

    private bool ExisteMesaComMesmoNumero(int numero, Guid? idIgnorado = null)
    {
        return repositorioMesa
            .SelecionarTodos()
            .Any(m => m.Id != idIgnorado && m.Numero == numero);
    }
}
