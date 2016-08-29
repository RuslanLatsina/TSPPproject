using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(TSPPproject.Startup))]
namespace TSPPproject
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
