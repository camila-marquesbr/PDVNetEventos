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
using System.Net.Http;
using PDVNetEventos.Services.Cep;


namespace PDVNetEventos.Views
{
    public partial class cadastroParticipantes : Window
    {
        public cadastroParticipantes()
        {
            InitializeComponent();
            this.DataContext = new cadastroParticipanteViewModel();
        }
    }
}

