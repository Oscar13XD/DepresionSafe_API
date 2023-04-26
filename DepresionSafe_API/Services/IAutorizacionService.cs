using DepresionSafe_API.Models.Custom;

namespace DepresionSafe_API.Services
{
    public interface IAutorizacionService
    {
        Task<AutorizacionResponse> DevolverToken(AutorizacionRequest autorizacion);
    }
}
