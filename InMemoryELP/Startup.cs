using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(InMemoryELP.Startup))]
namespace InMemoryELP
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
