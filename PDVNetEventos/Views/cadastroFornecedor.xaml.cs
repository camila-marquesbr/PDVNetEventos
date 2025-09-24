using System;
using System.Net.Http;
using System.Windows;
using PDVNetEventos.Services.Cep;
using PDVNetEventos.ViewModels;

namespace PDVNetEventos.Views
{
    public partial class cadastroFornecedor : Window
    {
        public cadastroFornecedor(ICepService cepService)
        {
            InitializeComponent();
            if (cepService == null) throw new ArgumentNullException(nameof(cepService));

            DataContext = new cadastroFornecedorViewModel(cepService);
        }

        // fallback para o Designer/preview
        public cadastroFornecedor()
            : this(new ViaCepService(new HttpClient { BaseAddress = new Uri("https://viacep.com.br/") }))
        { }
    }
}