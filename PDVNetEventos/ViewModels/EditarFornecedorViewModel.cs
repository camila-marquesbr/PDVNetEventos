using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class EditarFornecedorViewModel : INotifyPropertyChanged
    {
        public int Id { get; }
        private string _servico = "", _cnpj = "";
        private decimal? _precoPadrao;
        public string NomeServico { get => _servico; set { _servico = value; OnPropertyChanged(nameof(NomeServico)); } }
        public string CNPJ { get => _cnpj; set { _cnpj = value; OnPropertyChanged(nameof(CNPJ)); } }
        public decimal? PrecoPadrao { get => _precoPadrao; set { _precoPadrao = value; OnPropertyChanged(nameof(PrecoPadrao)); } }

        public ICommand SalvarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarFornecedorViewModel(int id)
        {
            Id = id;
            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            CancelarCommand = new RelayCommand(_ => Close());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();
            var f = await db.Fornecedores.AsNoTracking().FirstAsync(x => x.Id == Id);
            NomeServico = f.NomeServico;
            CNPJ = f.CNPJ;
            PrecoPadrao = f.PrecoPadrao;
        }

        private async Task SalvarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AtualizarFornecedorAsync(new Fornecedor
                {
                    Id = Id,
                    NomeServico = NomeServico,
                    CNPJ = CNPJ,
                    PrecoPadrao = PrecoPadrao
                });
                MessageBox.Show("Fornecedor atualizado!");
                Close();
            }
            catch (System.Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void Close() =>
            Application.Current.Windows[Application.Current.Windows.Count - 1]?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}