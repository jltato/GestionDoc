namespace SUAP_PortalOficios.Extensions
{
    using Microsoft.Extensions.Logging;

    public static class LogExtensions
    {
        public static void Modificacion(this ILogger logger, string usuario, string descripcion)
        {
           logger.LogInformation("[MOD][{Usuario}]:{Descripcion}", usuario, descripcion);           
        }
    }

}
