using System.Windows;
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class RelSaldoEventos : Window
    {
        public RelSaldoEventos()
        {
            InitializeComponent();
            DataContext = new RelSaldoEventosViewModel();
        }
    }
}