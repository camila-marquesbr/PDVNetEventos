using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.ViewModels.Shared;

namespace PDVNetEventos.ViewModels
{
    public class ListarFornecedoresViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FornecedorLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }

        public ListarFornecedoresViewModel()
        {
            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            EditarCommand = new RelayCommand(f => Editar((FornecedorLinha)f!));
            ExcluirCommand = new RelayCommand(async f => await ExcluirAsync((FornecedorLinha)f!));
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            try
            {
                using var db = new AppDbContext();

                var lista = await db.Fornecedores
                    .AsNoTracking()
                    .OrderBy(f => f.NomeServico)
                    .Select(f => new FornecedorLinha
                    {
                        Id = f.Id,
                        NomeServico = f.NomeServico,
                        CNPJ = f.CNPJ,
                        PrecoPadrao = f.PrecoPadrao
                    })
                    .ToListAsync();

                Itens.Clear();
                foreach (var i in lista) Itens.Add(i);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar: " + ex.Message);
            }
        }

        private void Editar(FornecedorLinha f)
        {
            new PDVNetEventos.Views.EditarFornecedor(f.Id).ShowDialog();
            _ = CarregarAsync();
        }

        private async Task ExcluirAsync(FornecedorLinha f)
        {
            if (MessageBox.Show($"Excluir fornecedor '{f.NomeServico}'?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using var db = new AppDbContext();

                var v = db.EventosFornecedores.Where(x => x.FornecedorId == f.Id);
                db.RemoveRange(v);

                var ent = await db.Fornecedores.FindAsync(f.Id);
                if (ent != null) db.Fornecedores.Remove(ent);

                await db.SaveChangesAsync();
                Itens.Remove(f);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}