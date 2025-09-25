using System.Configuration;
using System.Data;
using System.Windows;
using PDVNetEventos.Services.Auth;
using PDVNetEventos.Views;

namespace PDVNetEventos
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // DI manual simples para o auth
            IAuthService auth = new MockAuthService();

            var login = new LoginWindow(auth);
            login.Show();
        }
    }
}
