using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DepresionSafe_API.Models;
using DepresionSafe_API.Models.Custom;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace DepresionSafe_API.Services
{
    public class AutorizacionService : IAutorizacionService
    {
        private readonly DepresionSafeContext _context;
        private readonly IConfiguration _configuration;

        public AutorizacionService(DepresionSafeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        private string GenerarToken(string idUsuario, string rol)
        {
            var key = _configuration.GetValue<string>("JwtSettings:Key");
            var keyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, idUsuario));
            claims.AddClaim(new Claim(ClaimTypes.Role, rol));

            var credencialesToken = new SigningCredentials(
                new SymmetricSecurityKey(keyBytes),
                SecurityAlgorithms.HmacSha256Signature
            );

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

            string tokenCreado = tokenHandler.WriteToken(tokenConfig);
            return tokenCreado;
        }

        public async Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion)
        {
            //ENCRYPTAMOS LA CLAVE
            autorizacion.Clave = codifica(autorizacion.Clave);

            var usuario = _context.Usuarios.Include(e => e.IdRolUsuarioNavigation).FirstOrDefault(x =>
                x.Correo == autorizacion.Correo && x.Password == autorizacion.Clave
            );
            if (usuario == null)
            {
                return await Task.FromResult<AutorizacionResponse>(null);
            }
            string tokenCreado = GenerarToken(usuario.Id.ToString(), usuario.IdRolUsuarioNavigation.Descripcion);
            return new AutorizacionResponse()
            {
                Token = tokenCreado,
                Resultado = true,
                Mensaje = "OK"
            };
        }

        //METODO ENCRYIPTACION CONTRASEÑA SHA256
        private static string ToHexString(byte[] array)
        {
            StringBuilder hex = new StringBuilder(array.Length * 2);
            foreach (byte b in array)
            {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
        private static string codifica(string valor)
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
    }
}
