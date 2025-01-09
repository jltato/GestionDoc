using SUAP_PortalOficios.Models.DTOs;
using System.Security.Claims;

namespace SUAP_PortalOficios.Data.Repository.Interfaces
{
    public interface IInternoRepository
    {
        public Task<InternoResultadoDTO?> BuscarInternoAsync(int legajo);
        public Task<InternoResultadoDTO?> BuscarInternoAsync(int legajo, ClaimsPrincipal user, bool ampliar);
        public Task<InternoResultadoDTO?> BuscarInternoAsync(int legajo, ClaimsPrincipal user, bool ampliar, bool enLibertad);
        public Task<List<InternoResultadoDTO?>?> BuscarInternoAsync(string busqueda, ClaimsPrincipal user);
        public Task<List<InternoResultadoDTO?>?> BuscarInternoAsync(string busqueda, ClaimsPrincipal user, bool ampliar);
        public Task<List<InternoResultadoDTO?>?> BuscarInternoAsync(string busqueda, ClaimsPrincipal user, bool ampliar, bool enLibertad);
    }
}
