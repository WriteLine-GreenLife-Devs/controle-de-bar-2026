using System.ComponentModel.DataAnnotations;

namespace ControleDeBar.Dominio.Modulos.ModuloMesa;

public enum StatusMesa
{
    [Display(Name = "Indeterminado")]
    Indeterminado = 0,

    [Display(Name = "Livre")]
    Livre = 1,

    [Display(Name = "Ocupada")]
    Ocupada = 2
}
