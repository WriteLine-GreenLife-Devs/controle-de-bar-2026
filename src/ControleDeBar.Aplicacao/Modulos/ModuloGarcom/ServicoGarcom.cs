using ControleDeBar.Aplicacao.Compartilhado;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using FluentResults;

namespace ControleDeBar.Aplicacao.Modulos.ModuloGarcom;

public class ServicoGarcom(
    IRepositorioGarcom repositorioGarcom
) : ServicoBase<Garcom>
{
    public Result Cadastrar(CadastrarGarcomDto dto)
    {
        string nomeNormalizado = NormalizarNome(dto.Nome);

        if (ExisteGarcomComMesmoNome(nomeNormalizado))
            return Falha(nameof(dto.Nome), "Já existe um garçom com este nome.");

        Garcom novoGarcom = new(nomeNormalizado);

        Result resultadoValidacao = ValidarEntidade(novoGarcom);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        repositorioGarcom.Cadastrar(novoGarcom);

        return Result.Ok();
    }

    public Result Editar(EditarGarcomDto dto)
    {
        string nomeNormalizado = NormalizarNome(dto.Nome);

        if (ExisteGarcomComMesmoNome(nomeNormalizado, dto.Id))
            return Falha(nameof(dto.Nome), "Já existe um garçom com este nome.");

        Garcom garcomAtualizado = new(nomeNormalizado);

        Result resultadoValidacao = ValidarEntidade(garcomAtualizado);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        bool conseguiuEditar = repositorioGarcom.Editar(dto.Id, garcomAtualizado);

        if (!conseguiuEditar)
            return Falha(string.Empty, "Garçom não encontrado.");

        return Result.Ok();
    }

    public Result Excluir(Guid id)
    {
        Garcom? garcom = repositorioGarcom.SelecionarPorId(id);

        if (garcom is null)
            return Falha(string.Empty, "Garçom não encontrado.");

        repositorioGarcom.Excluir(id);

        return Result.Ok();
    }

    public List<ListarGarcomDto> SelecionarTodos()
    {
        return repositorioGarcom
            .SelecionarTodos()
            .Select(g => new ListarGarcomDto(g.Id, g.Nome))
            .ToList();
    }

    public Result<DetalhesGarcomDto> SelecionarPorId(Guid id)
    {
        Garcom? garcom = repositorioGarcom.SelecionarPorId(id);

        if (garcom is null)
            return Result.Fail("Garçom não encontrado.");

        DetalhesGarcomDto dto = new(garcom.Id, garcom.Nome);

        return Result.Ok(dto);
    }

    private bool ExisteGarcomComMesmoNome(string nome, Guid? idIgnorado = null)
    {
        return repositorioGarcom
            .SelecionarTodos()
            .Any(g =>
                g.Id != idIgnorado &&
                string.Equals(g.Nome, nome, StringComparison.OrdinalIgnoreCase)
            );
    }

    private static string NormalizarNome(string nome)
    {
        return nome?.Trim() ?? string.Empty;
    }
}
