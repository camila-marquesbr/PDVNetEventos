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

namespace PDVNetEventos.ViewModels
{
    public class listarEventosViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventoLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand AbrirParticipantesCommand { get; }
        public ICommand AbrirFornecedoresCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }

        private bool _carregando;
        public bool Carregando
        {
            get => _carregando;
            set { _carregando = value; OnPropertyChanged(nameof(Carregando)); }
        }

        public listarEventosViewModel()
        {
            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            AbrirParticipantesCommand = new RelayCommand(e => AbrirParticipantes((EventoLinha)e!));
            AbrirFornecedoresCommand = new RelayCommand(e => AbrirFornecedores((EventoLinha)e!));
            EditarCommand = new RelayCommand(e => EditarEvento((EventoLinha)e!));
            ExcluirCommand = new RelayCommand(async e => await ExcluirAsync((EventoLinha)e!));

            _ = CarregarAsync();
        }

        public async Task CarregarAsync()
        {
            try
            {
                Carregando = true;
                using var db = new AppDbContext();

                var query =
                    from e in db.Eventos.AsNoTracking()
                    select new EventoLinha
                    {
                        Id = e.Id,
                        Nome = e.Nome,
                        DataInicio = e.DataInicio,
                        DataFim = e.DataFim,
                        Capacidade = e.CapacidadeMaxima,
                        Orcamento = e.OrcamentoMaximo,
                        Inscritos = db.EventosParticipantes.Count(ep => ep.EventoId == e.Id),
                        Gasto = (decimal?)db.EventosFornecedores
                                        .Where(ef => ef.EventoId == e.Id)
                                        .Sum(ef => (decimal?)ef.ValorAcordado) ?? 0m
                    };

                var lista = await query.OrderBy(x => x.DataInicio).ToListAsync();

                Itens.Clear();
                foreach (var i in lista)
                {
                    i.Saldo = i.Orcamento - i.Gasto;
                    Itens.Add(i);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar: " + ex.Message);
            }
            finally { Carregando = false; }
        }

        private void AbrirParticipantes(EventoLinha e)
            => new PDVNetEventos.Views.ListarParticipantesDoEvento(e.Id, e.Nome).ShowDialog();

        private void AbrirFornecedores(EventoLinha e)
            => new PDVNetEventos.Views.ListarFornecedoresDoEvento(e.Id, e.Nome).ShowDialog();

        private void EditarEvento(EventoLinha e)
            => new PDVNetEventos.Views.EditarEvento(e.Id).ShowDialog();

        private async Task ExcluirAsync(EventoLinha e)
        {
            if (MessageBox.Show($"Excluir o evento '{e.Nome}'?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using var db = new AppDbContext();

                db.RemoveRange(db.EventosParticipantes.Where(x => x.EventoId == e.Id));
                db.RemoveRange(db.EventosFornecedores.Where(x => x.EventoId == e.Id));

                var ev = await db.Eventos.FindAsync(e.Id);
                if (ev != null) db.Eventos.Remove(ev);

                await db.SaveChangesAsync();
                Itens.Remove(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }

    public class EventoLinha
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int Capacidade { get; set; }
        public decimal Orcamento { get; set; }
        public int Inscritos { get; set; }
        public decimal Gasto { get; set; }
        public decimal Saldo { get; set; }
    }
}