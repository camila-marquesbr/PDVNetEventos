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
    public class ListarFornecedoresDoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public string Titulo { get; }
        public ObservableCollection<FornecedorDoEventoLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand RemoverCommand { get; }

        public ListarFornecedoresDoEventoViewModel(int eventoId, string nomeEvento)
        {
            _eventoId = eventoId;
            Titulo = $"Fornecedores do evento: {nomeEvento}";

            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            RemoverCommand = new RelayCommand(async f => await RemoverAsync((FornecedorDoEventoLinha)f!));

            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var lista = await db.EventosFornecedores
                .AsNoTracking()
                .Where(ef => ef.EventoId == _eventoId)
                .Select(ef => new FornecedorDoEventoLinha
                {
                    Id = ef.Fornecedor.Id,
                    NomeServico = ef.Fornecedor.NomeServico,
                    CNPJ = ef.Fornecedor.CNPJ,
                    ValorAcordado = ef.ValorAcordado
                })
                .OrderBy(x => x.NomeServico)
                .ToListAsync();

            Itens.Clear();
            foreach (var f in lista) Itens.Add(f);
        }

        private async Task RemoverAsync(FornecedorDoEventoLinha f)
        {
            if (MessageBox.Show($"Remover '{f.NomeServico}' deste evento?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();

            var link = await db.EventosFornecedores
                .FirstOrDefaultAsync(x => x.EventoId == _eventoId && x.FornecedorId == f.Id);

            if (link != null)
            {
                db.EventosFornecedores.Remove(link);
                await db.SaveChangesAsync();
            }

            await CarregarAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class FornecedorDoEventoLinha
    {
        public int Id { get; set; }
        public string NomeServico { get; set; } = "";
        public string CNPJ { get; set; } = "";
        public decimal ValorAcordado { get; set; }
    }
}