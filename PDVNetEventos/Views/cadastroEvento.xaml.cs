using PDVNetEventos.Services.Cep;
using PDVNetEventos.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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


namespace PDVNetEventos.Views
{
    public partial class cadastroEvento : Window
    {
        private readonly ICepService _cepService;

        // Construtor principal: receberá da MainWindow
        public cadastroEvento(ICepService cepService)
        {
            InitializeComponent();
            _cepService = cepService ?? throw new ArgumentNullException(nameof(cepService));

        
            // se você já tinha um ViewModel próprio do Evento, use-o aqui e injete o serviço nele
            this.DataContext = new cadastroEventoViewModel(_cepService); // NOVO
        }

        // Construtor fallback p/ Designer/preview
        public cadastroEvento()
            : this(new ViaCepService(new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") }))
        { }
    }
}