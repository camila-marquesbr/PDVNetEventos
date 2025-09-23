using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PDVNetEventos.Commands;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class EditarFornecedorViewModel : INotifyPropertyChanged
    {
        private readonly int _id;

        public string NomeServico { get; set; } = "";
        public string CNPJ { get; set; } = "";
        public decimal? PrecoPadrao { get; set; }

        public ICommand SalvarCommand { get; }

        public EditarFornecedorViewModel(int id)
        {
            _id = id;
            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            var svc = new EventService();
            var f = await svc.ObterFornecedorAsync(_id);

            NomeServico = f.NomeServico;
            CNPJ = f.CNPJ;
            PrecoPadrao = f.PrecoPadrao;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private async Task SalvarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AtualizarFornecedorAsync(new Fornecedor
                {
                    Id = _id,
                    NomeServico = NomeServico,
                    CNPJ = CNPJ,
                    PrecoPadrao = PrecoPadrao
                });

                MessageBox.Show("Fornecedor atualizado!");
                Fechar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void Fechar()
            => System.Windows.Application.Current.Windows[0]?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}