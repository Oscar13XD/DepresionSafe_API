using DepresionSafe_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace DepresionSafe_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : Controller
    {
        private readonly DepresionSafeContext _context;

        public UserController(DepresionSafeContext context)
        {
            _context = context;
        }
        //METODO PARA OBTENER LA LISTA DE USUARIOS
        [HttpGet]
        [Route("TraerUsuarios")]
        [Authorize]
        public async Task<IActionResult> TraerUsuarios()
        {

            var usuarios = await _context.Usuarios.Include(e => e.IdRolUsuarioNavigation).Select(e => new
            {
                e.Id,
                e.Nombre,
                e.Cedula,
                e.Telefono,
                Rol = e.IdRolUsuarioNavigation.Descripcion
            }).ToListAsync();

            if (usuarios.Count > 0)
            {
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", Usuarios = usuarios });
            }
            return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "no hay usuarios" });
        }
    }
}
