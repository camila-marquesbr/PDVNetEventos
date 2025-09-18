using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PDVNetEventos.Views;

namespace PDVNetEventos
{
    public partial class MainWindow : Window
    {
        public MainWindow() { InitializeComponent(); }

        private void AbrirCadastroEvento_Click(object sender, RoutedEventArgs e)
        {
            new cadastroEvento().ShowDialog();
        }


        private void AbrirCadastroParticipante_Click(object sender, RoutedEventArgs e)
        {
            new PDVNetEventos.Views.cadastroParticipantes().ShowDialog();
        }

        private void AbrirCadastroFornecedor_Click(object sender, RoutedEventArgs e)
        {
            new PDVNetEventos.Views.cadastroFornecedor().ShowDialog();
        }

        private void AbrirListarEventos_Click(object sender, RoutedEventArgs e)
        {
            new PDVNetEventos.Views.ListarEventos().ShowDialog();
        }
    }
}