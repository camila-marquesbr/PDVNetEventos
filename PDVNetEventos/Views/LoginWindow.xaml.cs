using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using PDVNetEventos.Services.Auth;

namespace PDVNetEventos.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthService _auth;

        public LoginWindow(IAuthService auth)
        {
            InitializeComponent();
            _auth = auth;
        }

        private void Entrar_Click(object sender, RoutedEventArgs e)
        {
            var token = _auth.Login(TxtUser.Text?.Trim() ?? "", TxtPass.Password ?? "");
            if (token == null)
            {
                LblStatus.Text = "Usuário ou senha inválidos.";
                return;
            }

            // sucesso -> abre MainWindow
            var main = new MainWindow();
            main.Show();
            this.Close();
        }

        private void Sair_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}