using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
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
using Microsoft.EntityFrameworkCore;

namespace PDVNetEventos.Views
{
    public partial class VincularParticipanteAoEvento : Window
    {
        public VincularParticipanteAoEvento(int eventoId)
        {
            InitializeComponent();
            DataContext = new PDVNetEventos.ViewModels.VincularParticipanteAoEventoViewModel(eventoId);
        }
    }
}