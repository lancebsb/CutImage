using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CutImage.Startup))]
namespace CutImage
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
