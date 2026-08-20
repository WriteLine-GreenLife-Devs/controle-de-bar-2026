using ControleDeBar.Dominio.Compartilhado.Identity;

namespace ControleDeBar.Testes.Integracao.Compartilhado.Identity;

public sealed class ProvedorDeUsuarioFake(Guid userId) : IProvedorDeUsuario
{
    public Guid? Id => userId;

    public bool EstaAutenticado => true;
}
