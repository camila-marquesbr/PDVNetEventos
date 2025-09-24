using System;
using System.Net.Http;
using System.Windows;
using PDVNetEventos.Services.Cep;
using PDVNetEventos.Views;

namespace PDVNetEventos
{
    public partial class MainWindow : Window
    {
        private readonly ICepService _cepService;

        public MainWindow()
        {
            InitializeComponent();

            // CEP segue para Evento apenas
            var http = new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") };
            _cepService = new ViaCepService(http);
        }

        private void AbrirCadastroEvento_Click(object sender, RoutedEventArgs e)
        {
            new cadastroEvento(_cepService).ShowDialog();
        }

        private void AbrirCadastroParticipante_Click(object sender, RoutedEventArgs e)
        {
            new cadastroParticipantes().ShowDialog();
        }

        private void AbrirCadastroFornecedor_Click(object sender, RoutedEventArgs e)
        {
            new cadastroFornecedor().ShowDialog(); 
        }

        private void AbrirListarEventos_Click(object sender, RoutedEventArgs e)
        {
            new ListarEventos().ShowDialog();
        }
    }
}