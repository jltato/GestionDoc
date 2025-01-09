namespace SUAP_PortalOficios.Views.Shared.Components.Services
{
    using System.Net;
    using System.Net.Http;
    using Microsoft.AspNetCore.Antiforgery;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Components;

    public class HttpClientService
    {
        private readonly NavigationManager _navigationManager;
        private readonly IAntiforgery _antiforgery;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpClientService(NavigationManager navigationManager, IAntiforgery antiforgery, IHttpContextAccessor httpContextAccessor)
        {
            _navigationManager = navigationManager;
            _antiforgery = antiforgery;
            _httpContextAccessor = httpContextAccessor;
        }

        public HttpClient CreateConfiguredHttpClient()
        {
            var handler = new HttpClientHandler();
            var cookieContainer = new CookieContainer();
            handler.CookieContainer = cookieContainer;
            var baseUri = new Uri(_navigationManager.BaseUri);

            // Obtener el token de antifalsificación
            var tokens = _antiforgery.GetAndStoreTokens(_httpContextAccessor.HttpContext);
            var antiforgeryToken = tokens.RequestToken;

            // Obtener las cookies de Identity y Antiforgery desde HttpContext
            var identityCookie = _httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Identity.Application"];
            var antiforgeryCookie = _httpContextAccessor.HttpContext.Request.Cookies[".AspNetCore.Antiforgery"];

            if (!string.IsNullOrEmpty(identityCookie))
            {
                cookieContainer.Add(baseUri, new Cookie(".AspNetCore.Identity.Application", identityCookie));
            }
            if (!string.IsNullOrEmpty(antiforgeryCookie))
            {
                cookieContainer.Add(baseUri, new Cookie(".AspNetCore.Antiforgery", antiforgeryCookie));
            }

            var client = new HttpClient(handler) { BaseAddress = baseUri, MaxResponseContentBufferSize = 10_000_000 }; //10 MB
            client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", antiforgeryToken);

            return client;
        }
    }

}
