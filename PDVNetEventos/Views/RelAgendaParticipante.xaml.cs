using System.Windows;
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class RelAgendaParticipante : Window
    {
        public RelAgendaParticipante()
        {
            InitializeComponent();
            DataContext = new RelAgendaParticipanteViewModel();
        }
    }
}