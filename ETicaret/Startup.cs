using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ETicaret.Startup))]
namespace ETicaret
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
