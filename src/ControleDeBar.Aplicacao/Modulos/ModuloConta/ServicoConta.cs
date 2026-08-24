using ControleDeBar.Aplicacao.Compartilhado;
using ControleDeBar.Dominio.Modulos.ModuloConta;
using ControleDeBar.Dominio.Modulos.ModuloGarcom;
using ControleDeBar.Dominio.Modulos.ModuloMesa;
using FluentResults;

namespace ControleDeBar.Aplicacao.Modulos.ModuloConta;

public class ServicoConta(
    IRepositorioConta repositorioConta,
    IRepositorioMesa repositorioMesa,
    IRepositorioGarcom repositorioGarcom
) : ServicoBase<Conta>
{
    public Result Abrir(AbrirContaDto dto)
    {
        Mesa? mesa = repositorioMesa.SelecionarPorId(dto.MesaId);

        if (mesa is null)
            return Falha(nameof(dto.MesaId), "Mesa não encontrada.");

        Garcom? garcom = repositorioGarcom.SelecionarPorId(dto.GarcomId);

        if (garcom is null)
            return Falha(nameof(dto.GarcomId), "Garçom não encontrado.");

        Conta novaConta = new(
            dto.MesaId,
            dto.GarcomId,
            dto.NomeCliente
        );

        Result resultadoValidacao = ValidarEntidade(novaConta);

        if (resultadoValidacao.IsFailed)
            return resultadoValidacao;

        mesa.Ocupar();

        repositorioConta.Cadastrar(novaConta);

        return Result.Ok();
    }

    public Result Fechar(Guid id)
    {
        Conta? conta = repositorioConta.SelecionarPorId(id);

        if (conta is null)
            return Falha(string.Empty, "Conta não encontrada.");

        if (conta.Status == StatusConta.Fechada)
            return Falha(string.Empty, "A conta já está fechada.");

        conta.Fechar();

        bool existemOutrasContasAbertas = repositorioConta
            .SelecionarTodos()
            .Any(c =>
                c.Id != conta.Id &&
                c.MesaId == conta.MesaId &&
                c.Status == StatusConta.Aberta
            );

        if (!existemOutrasContasAbertas)
        {
            Mesa? mesa = repositorioMesa.SelecionarPorId(conta.MesaId);

            if (mesa is not null)
                mesa.Liberar();
        }

        repositorioConta.Salvar();

        return Result.Ok();
    }

    public List<ListarContaDto> SelecionarAbertas()
    {
        return repositorioConta
            .SelecionarTodos()
            .Where(c => c.Status == StatusConta.Aberta)
            .Select(MapearParaListagem)
            .ToList();
    }

    public List<ListarContaDto> SelecionarFechadas()
    {
        return repositorioConta
            .SelecionarTodos()
            .Where(c => c.Status == StatusConta.Fechada)
            .Select(MapearParaListagem)
            .ToList();
    }

    public List<ListarContaDto> SelecionarTodos()
    {
        return repositorioConta
            .SelecionarTodos()
            .Select(MapearParaListagem)
            .ToList();
    }

    public Result<DetalhesContaDto> SelecionarPorId(Guid id)
    {
        Conta? conta = repositorioConta.SelecionarPorId(id);

        if (conta is null)
            return Result.Fail("Conta não encontrada.");

        Mesa? mesa = repositorioMesa.SelecionarPorId(conta.MesaId);
        Garcom? garcom = repositorioGarcom.SelecionarPorId(conta.GarcomId);

        if (mesa is null || garcom is null)
            return Result.Fail("Não foi possível carregar os dados da conta.");

        DetalhesContaDto dto = new(
            conta.Id,
            conta.MesaId,
            mesa.Numero,
            conta.GarcomId,
            garcom.Nome,
            conta.NomeCliente,
            conta.DataAbertura,
            conta.DataFechamento,
            conta.Status
        );

        return Result.Ok(dto);
    }

    private ListarContaDto MapearParaListagem(Conta conta)
    {
        Mesa? mesa = repositorioMesa.SelecionarPorId(conta.MesaId);
        Garcom? garcom = repositorioGarcom.SelecionarPorId(conta.GarcomId);

        return new ListarContaDto(
            conta.Id,
            conta.NomeCliente,
            mesa?.Numero ?? 0,
            garcom?.Nome ?? string.Empty,
            conta.DataAbertura,
            conta.DataFechamento,
            conta.Status
        );
    }
}
