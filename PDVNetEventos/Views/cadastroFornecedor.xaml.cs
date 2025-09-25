using System;
using System.Net.Http;
using System.Windows;
using PDVNetEventos.Services.Cep;
using PDVNetEventos.ViewModels;



namespace PDVNetEventos.Views
{
    public partial class cadastroFornecedor : Window
    {
        public cadastroFornecedor()
        {
            InitializeComponent();
            DataContext = new cadastroFornecedorViewModel();
        }
    }
}