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
    public class ListarParticipantesDoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public ObservableCollection<ParticipanteLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand RemoverVinculoCommand { get; }


        public ListarParticipantesDoEventoViewModel(int eventoId, string? _nomeIgnorado = null)
        {
            _eventoId = eventoId;

            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            RemoverVinculoCommand = new RelayCommand(async p => await RemoverVinculoAsync((ParticipanteLinha)p!));

            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var query =
                from ep in db.EventosParticipantes.AsNoTracking()
                where ep.EventoId == _eventoId
                join p in db.Participantes.AsNoTracking() on ep.ParticipanteId equals p.Id
                orderby p.NomeCompleto
                select new ParticipanteLinha
                {
                    Id = p.Id,
                    NomeCompleto = p.NomeCompleto,
                    CPF = p.CPF,
                    Tipo = p.Tipo
                };

            var lista = await query.ToListAsync();

            Itens.Clear();
            foreach (var i in lista)
                Itens.Add(i);
        }

        private async Task RemoverVinculoAsync(ParticipanteLinha? p)
        {
            if (p is null) return;

            if (MessageBox.Show($"Remover '{p.NomeCompleto}' do evento?",
                    "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            using var db = new AppDbContext();
            var vinc = await db.EventosParticipantes
                .FirstOrDefaultAsync(x => x.EventoId == _eventoId && x.ParticipanteId == p.Id);

            if (vinc != null)
            {
                db.EventosParticipantes.Remove(vinc);
                await db.SaveChangesAsync();
                await CarregarAsync();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}