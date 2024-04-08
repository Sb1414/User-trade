using System.Web.Http;
using System.Web.Http.Cors;

namespace KalkamanovaFinalWeb
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Enable CORS
            config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            // Other Web API configuration not shown...
        }
    }
}