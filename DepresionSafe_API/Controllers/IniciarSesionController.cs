using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DepresionSafe_API.Models.Custom;
using DepresionSafe_API.Services;

namespace DepresionSafe_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IniciarSesionController : ControllerBase
    {
        private readonly IAutorizacionService _autorizacionService;

        public IniciarSesionController(IAutorizacionService autorizacionService)
        {
            _autorizacionService = autorizacionService;
        }

        [HttpPost]
        [Route("Iniciar Sesion")]
        public async Task<IActionResult> IniciarSesion([FromBody] AutorizacionRequest autorizacion)
        {
            var resultado_autorizacion = await _autorizacionService.DevolverToken(autorizacion);
            if(resultado_autorizacion == null)
            {
                return Unauthorized();
            }
            return Ok(resultado_autorizacion);
        }
    }
}
