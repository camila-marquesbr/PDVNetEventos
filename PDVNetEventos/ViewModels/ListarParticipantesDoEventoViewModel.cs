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
    public class ListarParticipantesDoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public string Titulo { get; }
        public ObservableCollection<ParticipanteDoEventoLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand RemoverCommand { get; }

        public ListarParticipantesDoEventoViewModel(int eventoId, string nomeEvento)
        {
            _eventoId = eventoId;
            Titulo = $"Participantes do evento: {nomeEvento}";

            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            RemoverCommand = new RelayCommand(async p => await RemoverAsync((ParticipanteDoEventoLinha)p!));

            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var lista = await db.EventosParticipantes
                .AsNoTracking()
                .Where(ep => ep.EventoId == _eventoId)
                .Select(ep => new ParticipanteDoEventoLinha
                {
                    Id = ep.Participante.Id,
                    Nome = ep.Participante.NomeCompleto,
                    CPF = ep.Participante.CPF,
                    Tipo = ep.Participante.Tipo.ToString()
                })
                .OrderBy(x => x.Nome)
                .ToListAsync();

            Itens.Clear();
            foreach (var p in lista) Itens.Add(p);
        }

        private async Task RemoverAsync(ParticipanteDoEventoLinha p)
        {
            if (MessageBox.Show($"Remover '{p.Nome}' deste evento?",
                "Confirmação", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            using var db = new AppDbContext();

            var link = await db.EventosParticipantes
                .FirstOrDefaultAsync(x => x.EventoId == _eventoId && x.ParticipanteId == p.Id);

            if (link != null)
            {
                db.EventosParticipantes.Remove(link);
                await db.SaveChangesAsync();
            }

            await CarregarAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }

    public class ParticipanteDoEventoLinha
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string CPF { get; set; } = "";
        public string Tipo { get; set; } = "";
    }
}