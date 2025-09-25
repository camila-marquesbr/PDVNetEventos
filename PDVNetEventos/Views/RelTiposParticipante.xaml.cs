using System.Windows;
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class RelTiposParticipante : Window
    {
        public RelTiposParticipante()
        {
            InitializeComponent();
            DataContext = new RelTiposParticipanteViewModel();
        }
    }
}