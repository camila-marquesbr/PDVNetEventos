using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Models;          // <— usa a classe da pasta Models

namespace PDVNetEventos.ViewModels
{
    public class ListarParticipantesViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public ObservableCollection<ParticipanteLinha> Itens { get; } = new();

        private ICommand? _removerVinculoCommand;
        public ICommand RemoverVinculoCommand =>
            _removerVinculoCommand ??= new RelayCommand(async p => await RemoverVinculoAsync(p as ParticipanteLinha));

        public event PropertyChangedEventHandler? PropertyChanged;

        // Segundo parâmetro só para casar com o construtor chamado pela View
        public ListarParticipantesViewModel(int eventoId, string? _ignored = null)
        {
            _eventoId = eventoId;
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var query =
                from ep in db.EventosParticipantes.AsNoTracking()
                where ep.EventoId == _eventoId
                join p in db.Participantes.AsNoTracking()
                    on ep.ParticipanteId equals p.Id
                orderby p.NomeCompleto
                select new ParticipanteLinha
                {
                    Id = p.Id,
                    NomeCompleto = p.NomeCompleto,
                    CPF = p.CPF,
                    Tipo = p.Tipo.ToString()
                };

            var lista = await query.ToListAsync();

            Itens.Clear();
            foreach (var i in lista)
                Itens.Add(i);
        }

        private async Task RemoverVinculoAsync(ParticipanteLinha? p)
        {
            if (p is null) return;

            using var db = new AppDbContext();

            var vinc = await db.EventosParticipantes
                .FirstOrDefaultAsync(x => x.EventoId == _eventoId && x.ParticipanteId == p.Id);

            if (vinc != null)
            {
                db.EventosParticipantes.Remove(vinc);
                await db.SaveChangesAsync();
                Itens.Remove(p);
            }
        }

        private void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}