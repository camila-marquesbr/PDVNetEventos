using System.Windows;
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class RelTopFornecedores : Window
    {
        public RelTopFornecedores()
        {
            InitializeComponent();
            DataContext = new RelTopFornecedoresViewModel();
        }
    }
}