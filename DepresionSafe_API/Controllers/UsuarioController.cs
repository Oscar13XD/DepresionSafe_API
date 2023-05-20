using DepresionSafe_API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace DepresionSafe_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuarioController : ControllerBase
    {
        private readonly DepresionSafeContext _context;

        public UsuarioController(DepresionSafeContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("ActualizarDatos")]
        [Authorize(Roles = "USUARIO")]
        public async Task<IActionResult> ActualizarDatos([FromBody] Usuario usuario)
        {
            if (User.Identity.IsAuthenticated)
            {
                usuario.Id = Convert.ToInt32(User.Claims.ToList()[0].Value);

                var user = await _context.Usuarios.FindAsync(usuario.Id);

                if (user != null)
                {
                    if (usuario.Correo != user.Correo)
                    {
                        //VALIDAR QUE EL CORREO NO EXISTA 
                        var userEmail = await _context.Usuarios.Where(b => b.Correo == usuario.Correo).FirstOrDefaultAsync();
                        if (userEmail != null)
                        {
                            return StatusCode(StatusCodes.Status302Found, new { mensaje = "email existente" });
                        }
                    }
                    if (usuario.Cedula != user.Cedula)
                    {
                        //VALIDAMOS DE QUE EL DOCUMENTO NO EXISTA
                        var userDocumento = await _context.Usuarios.Where(b => b.Cedula == usuario.Cedula).FirstOrDefaultAsync();
                        if (userDocumento != null)
                        {
                            return StatusCode(StatusCodes.Status302Found, new { mensaje = "documento existente" });
                        }
                    }
                    user.Nombre = usuario.Nombre;
                    user.Cedula = usuario.Cedula;
                    user.Telefono = usuario.Telefono;
                    user.Correo = usuario.Correo;
                    user.Password = codifica(usuario.Password) == "" ? user.Password : codifica(usuario.Password);
                

                    try
                    {
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                        return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
                    }
                }

            }
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "usuario no encontrado" });
        }

        [HttpGet]
        [Route("TraerDatos")]
        [Authorize(Roles = "USUARIO")]
        public async Task<IActionResult> TraerDatos()
        {
            if (User.Identity.IsAuthenticated)
            {
                int Id = Convert.ToInt32(User.Claims.ToList()[0].Value);

                var usuario = await _context.Usuarios.FindAsync(Id);
                if (usuario != null)
                {
                    return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok", usuario = usuario });
                }
                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "usuario no encontrado" });
            }
            return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = "usuario no encontrado" });
        }

        //ENCRYPTAR EL TOKEN
        private string GetSha256(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            ASCIIEncoding encoding = new ASCIIEncoding();
            Byte[] stream = null;
            StringBuilder sb = new StringBuilder();
            stream = sha256.ComputeHash(encoding.GetBytes(str));
            for (int i = 0; i < stream.Length; i++)
            {
                sb.AppendFormat("{0:x2}", stream[i]);
            }
            return sb.ToString();
        }

        private string codifica(string valor)
        {
            string hash;
            string llave = "6v+h*+jb!+91psuc%lj8ty(ql*fx-8(1remclj(ch5=fd-5-";
            ASCIIEncoding encoder = new ASCIIEncoding();
            Byte[] code = encoder.GetBytes(llave);
            using (HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(llave)))
            {
                Byte[] hmBytes = hmac.ComputeHash(encoder.GetBytes(valor));
                hash = ToHexString(hmBytes);
            }
            return hash.ToUpper();
        }

        //METODO ENCRYIPTACION CONTRASEÑA SHA256
        private string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
    }
}
