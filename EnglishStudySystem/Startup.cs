using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(EnglishStudySystem.Startup))]
namespace EnglishStudySystem
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
