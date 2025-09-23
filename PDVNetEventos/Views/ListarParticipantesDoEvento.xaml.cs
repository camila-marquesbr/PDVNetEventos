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
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class ListarParticipantesDoEvento : Window
    {
        public ListarParticipantesDoEvento(int eventoId, string eventoNome)
        {
            InitializeComponent();
            DataContext = new ListarParticipantesDoEventoViewModel(eventoId, eventoNome);
        }
    }
}
