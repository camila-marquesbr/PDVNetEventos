using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;

namespace PDVNetEventos.ViewModels
{
    public class ListarFornecedoresViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<FornecedorGeralLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }

        public ListarFornecedoresViewModel()
        {
            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            EditarCommand = new RelayCommand(f => Editar((FornecedorGeralLinha)f!));
            ExcluirCommand = new RelayCommand(async f => await ExcluirAsync((FornecedorGeralLinha)f!));

            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var lista = await db.Fornecedores.AsNoTracking()
                .Select(f => new FornecedorGeralLinha
                {
                    Id = f.Id,
                    NomeServico = f.NomeServico,
                    CNPJ = f.CNPJ,
                    PrecoPadrao = f.PrecoPadrao
                })
                .OrderBy(f => f.NomeServico)
                .ToListAsync();

            Itens.Clear();
            foreach (var f in lista) Itens.Add(f);
        }

        private void Editar(FornecedorGeralLinha f)
            => new PDVNetEventos.Views.EditarFornecedor(f.Id).ShowDialog();

        private async Task ExcluirAsync(FornecedorGeralLinha f)
        {
            if (MessageBox.Show($"Excluir fornecedor '{f.NomeServico}'?",
                    "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();

            db.RemoveRange(db.EventosFornecedores.Where(x => x.FornecedorId == f.Id));
            var forn = await db.Fornecedores.FindAsync(f.Id);
            if (forn != null) db.Fornecedores.Remove(forn);

            await db.SaveChangesAsync();
            await CarregarAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class FornecedorGeralLinha
    {
        public int Id { get; set; }
        public string NomeServico { get; set; } = "";
        public string CNPJ { get; set; } = "";
        public decimal? PrecoPadrao { get; set; }
    }
}