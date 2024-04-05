using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(KalkamanovaFinal.Startup))]
namespace KalkamanovaFinal
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
