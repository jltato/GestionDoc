using SUAP_PortalOficios.Models.DTOs;
using System.Security.Claims;

namespace SUAP_PortalOficios.Data.Repository.Interfaces
{
    public interface IUserPermissionsService
    {
        Task<UserPermissionsDTO> GetPermissionsAsync(ClaimsPrincipal user);
        void RemovePermissions(ClaimsPrincipal user);
    }
}
