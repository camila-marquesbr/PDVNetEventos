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
    public class ListarFornecedoresDoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        // Cabeçalho / estado
        private string _eventoNome = "";
        public string EventoNome
        {
            get => _eventoNome;
            private set { _eventoNome = value; OnPropertyChanged(nameof(EventoNome)); }
        }

        private decimal _gasto;
        public decimal Gasto
        {
            get => _gasto;
            private set { _gasto = value; OnPropertyChanged(nameof(Gasto)); OnPropertyChanged(nameof(Saldo)); }
        }

        private decimal _orcamento;
        public decimal Orcamento
        {
            get => _orcamento;
            private set { _orcamento = value; OnPropertyChanged(nameof(Orcamento)); OnPropertyChanged(nameof(Saldo)); }
        }

        public decimal Saldo => Orcamento - Gasto;

        private bool _carregando;
        public bool Carregando
        {
            get => _carregando;
            private set { _carregando = value; OnPropertyChanged(nameof(Carregando)); }
        }

        public ObservableCollection<FornecedorLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand RemoverVinculoCommand { get; }


        public ListarFornecedoresDoEventoViewModel(int eventoId, string? nomeEvento = null)
        {
            _eventoId = eventoId;
            if (!string.IsNullOrWhiteSpace(nomeEvento))
                EventoNome = nomeEvento;

            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            RemoverVinculoCommand = new RelayCommand(async f => await RemoverVinculoAsync((FornecedorLinha)f!));


            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            try
            {
                Carregando = true;
                using var db = new AppDbContext();

                var ev = await db.Eventos.AsNoTracking()
                    .Where(e => e.Id == _eventoId)
                    .Select(e => new { e.Nome, e.OrcamentoMaximo })
                    .FirstOrDefaultAsync();

                if (ev != null)
                {
                    Orcamento = ev.OrcamentoMaximo;
                    if (string.IsNullOrWhiteSpace(EventoNome))
                        EventoNome = ev.Nome;
                }

                var query =
                    from ef in db.EventosFornecedores.AsNoTracking()
                    where ef.EventoId == _eventoId
                    join f in db.Fornecedores.AsNoTracking() on ef.FornecedorId equals f.Id
                    orderby f.NomeServico
                    select new FornecedorLinha
                    {
                        Id = f.Id,
                        NomeServico = f.NomeServico,
                        CNPJ = f.CNPJ,
                        PrecoPadrao = f.PrecoPadrao,
                        ValorAcordado = ef.ValorAcordado
                    };

                var lista = await query.ToListAsync();

                Gasto = lista.Sum(x => x.ValorAcordado);

                Itens.Clear();
                foreach (var i in lista)
                    Itens.Add(i);

                Itens.Clear();
                foreach (var i in lista) Itens.Add(i);
            }
            finally
            {
                Carregando = false;
            }
        }

        private async Task RemoverVinculoAsync(FornecedorLinha? f)
        {
            if (f is null) return;

            if (MessageBox.Show($"Remover fornecedor '{f.NomeServico}' do evento?",
                    "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var vinc = await db.EventosFornecedores
                .FirstOrDefaultAsync(x => x.EventoId == _eventoId && x.FornecedorId == f.Id);

            if (vinc != null)
            {
                db.EventosFornecedores.Remove(vinc);
                await db.SaveChangesAsync();
                await CarregarAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}