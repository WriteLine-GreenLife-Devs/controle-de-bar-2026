using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ControleDeBar.WebApp.Compartilhado.Extensions;

public static class EnumExtensions
{
    public static string GetDisplayName(this Enum valor)
    {
        MemberInfo? membro = valor
            .GetType()
            .GetMember(valor.ToString())
            .FirstOrDefault();

        DisplayAttribute? atributo = membro?.GetCustomAttribute<DisplayAttribute>();

        return atributo?.GetName() ?? valor.ToString();
    }
}
