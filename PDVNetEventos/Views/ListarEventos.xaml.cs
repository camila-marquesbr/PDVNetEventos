using System.Windows;
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class ListarEventos : Window
    {
        public ListarEventos()
        {
            InitializeComponent();
            DataContext = new ListarEventosViewModel();
        }
    }
}