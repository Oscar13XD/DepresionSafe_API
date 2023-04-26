using System;
using System.Collections.Generic;

namespace DepresionSafe_API.Models;

public partial class Subscripcion
{
    public int Id { get; set; }

    public string Descripcion { get; set; } = null!;

    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
