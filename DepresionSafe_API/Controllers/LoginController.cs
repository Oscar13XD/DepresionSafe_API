using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DepresionSafe_API.Models.Custom;
using DepresionSafe_API.Services;
using DepresionSafe_API.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using System.Net.Mail;
using System.Net;

namespace DepresionSafe_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly DepresionSafeContext _context;
        private readonly IAutorizacionService _autorizacionService;

        public LoginController(DepresionSafeContext context, IAutorizacionService autorizacionService)
        {
            _context = context;
            _autorizacionService = autorizacionService;
        }

        //INICAR SESION
        [HttpPost]
        [Route("IniciarSesion")]
        public async Task<IActionResult> IniciarSesion([FromBody] AutorizacionRequest autorizacion)
        {
            var resultado_autorizacion = await _autorizacionService.DevolverToken(autorizacion);
            if (resultado_autorizacion == null)
            {
                return Unauthorized();
            }
            return Ok(resultado_autorizacion);
        }

        //METODO PARA REGISTRAR UN USUARIO
        [HttpPost]
        [Route("RegistrarUsuario")]
        public async Task<IActionResult> RegistrarUsuario([FromBody] Usuario usuario)
        {
            //VERIFICAMOS QUE NO EXISTA EL DOCUMENTO
            var Documento = await _context.Usuarios.Where(x => x.Cedula == usuario.Cedula).Select(e => new { e.Id }).ToListAsync();
            var Email = await _context.Usuarios.Where(x => x.Correo == usuario.Correo).Select(e => new { e.Id }).ToListAsync();
            if (Documento.Count > 0 && Email.Count > 0)
            {
                return StatusCode(StatusCodes.Status302Found, new { mensaje = "correo y documento existentes" });
            }
            if (Documento.Count > 0)
            {
                return StatusCode(StatusCodes.Status302Found, new { mensaje = "documento existente" });
            }
            if (Email.Count > 0)
            {
                return StatusCode(StatusCodes.Status302Found, new { mensaje = "email existente" });
            }

            //CODIFICAMOS LA CONTRASEÑA
            usuario.Password = codifica(usuario.Password);
            usuario.IdSubscripcion = 2;
            usuario.IdRolUsuario = 1;

            //REGISTRAMOS EL USUARIO EN LA BD
            try
            {
                _context.Add(usuario);
                _context.SaveChanges();
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
            }
        }

        //METODO PARA ENVIAR CORREO DE RECUPERACION
        [HttpPost]
        [Route("RecuperarPassword/{correo}")]
        public async Task<IActionResult> RecuperarPassword(string correo)
        {
            string token = GetSha256(Guid.NewGuid().ToString());
            var usuario = await _context.Usuarios.Where(b => b.Correo == correo).FirstOrDefaultAsync();
            if (usuario != null)
            {
                //ENVIAMOS EL CORREO
                try
                {
                    SendEmail(correo, token);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
                }

                //ACTUALIZAMOS EL TOKEN
                try
                {
                    usuario.Token = token;
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { mensaje = ex.Message });
                }

                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });

            }
            return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "usuario no encontrado" });
        }

        //METODO PARA VERIFICAR EL TOKEN DE REESTABLECER PASSWORD
        [HttpGet]
        [Route("VerificarToken/{token}")]
        public async Task<IActionResult> VerificarToken(string token)
        {
            var usuario = await _context.Usuarios.Where(b => b.Token == token).FirstOrDefaultAsync();
            if (usuario == null)
            {
                return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "usuario no encontrado" });
            }
            else
            {
                return StatusCode(StatusCodes.Status200OK, new { mensaje = "ok" });
            }
        }

        //METODO PARA RESSTABLECER LA CONTRASEÑA
        [HttpPut]
        [Route("ReestablecerPassword")]
        public async Task<IActionResult> ReestablecerPassword([FromBody] Usuario usuario)
        {
            var user = await _context.Usuarios.Where(x => x.Token == usuario.Token).FirstOrDefaultAsync();
            if (user != null)
            {
                //ACTUALIZAMOS VALORES
                user.Token = null;
                user.Password = usuario.Password;
                user.Password = codifica(user.Password);
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
            return StatusCode(StatusCodes.Status404NotFound, new { mensaje = "usuario no encontrado" });
        }


        //METODO PARA ENVIAR CORREOS
        private bool SendEmail(string Destinatario, string token)
        {
            string urlDomain = "http://localhost:5244/";
            string EmailOrigen = "depressionsafe23@gmail.com";
            string Password = "dlvwplgtawwfuttv";
            string url = urlDomain + "IniciarSesion/Reestablecer?token=" + token;

            //NOMBRE MENSAJE
            string Nombre = "DEPRESION SAFE";

            string Cuerpo = "" +
                 "<div>" +
                    "<a href=\"" + url + "\">Click Aqui Para reestablecer</a>" +
                 "</div>";
            string Asunto = "Reestablecer Contraseña";

            var mail = new MailMessage()
            {
                From = new MailAddress(EmailOrigen, Nombre),
                Subject = Asunto,
                Body = Cuerpo,
                BodyEncoding = System.Text.Encoding.UTF8,
                SubjectEncoding = System.Text.Encoding.Default,
                IsBodyHtml = true,

            };
            mail.To.Add(Destinatario.ToLower().Trim());
            var client = new SmtpClient()
            {
                EnableSsl = true,
                Port = 587,
                Host = "smtp.gmail.com",
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(EmailOrigen, Password)
            };

            //ENVÍAMOS CORREO
            client.Send(mail);
            client.Dispose();
            return true;
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
    }
}
