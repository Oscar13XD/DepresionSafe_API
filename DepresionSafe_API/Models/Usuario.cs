using System;
using System.Collections.Generic;

namespace DepresionSafe_API.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string Cedula { get; set; } = null!;

    public long Telefono { get; set; }

    public string Correo { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? IdSubscripcion { get; set; }

    public int IdRolUsuario { get; set; }

    public virtual RolUsuario IdRolUsuarioNavigation { get; set; } = null!;

    public virtual Subscripcion? IdSubscripcionNavigation { get; set; }
}
