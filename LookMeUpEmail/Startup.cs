using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(LookMeUpEmail.Startup))]
namespace LookMeUpEmail
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
