using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class RelAgendaParticipanteViewModel : INotifyPropertyChanged
    {
        private readonly RelatoriosService _svc = new();
        public ObservableCollection<Participante> Participantes { get; } = new();
        public ObservableCollection<AgendaLinha> Itens { get; } = new();

        private int _participanteId;
        public int ParticipanteId
        {
            get => _participanteId;
            set { _participanteId = value; OnPropertyChanged(nameof(ParticipanteId)); }
        }

        public ICommand CarregarAgendaCommand { get; }

        public RelAgendaParticipanteViewModel()
        {
            CarregarAgendaCommand = new RelayCommand(async _ => await CarregarAgendaAsync(), _ => ParticipanteId > 0);
            _ = CarregarParticipantesAsync();
        }

        private async Task CarregarParticipantesAsync()
        {
            using var db = new AppDbContext();
            var lista = await db.Participantes.AsNoTracking().OrderBy(p => p.NomeCompleto).ToListAsync();
            Participantes.Clear();
            foreach (var p in lista) Participantes.Add(p);
            if (Participantes.Any()) ParticipanteId = Participantes.First().Id;
        }

        private async Task CarregarAgendaAsync()
        {
            var dados = await _svc.ObterAgendaParticipanteAsync(ParticipanteId);
            Itens.Clear();
            foreach (var x in dados) Itens.Add(x);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}