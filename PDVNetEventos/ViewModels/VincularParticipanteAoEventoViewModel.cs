using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class VincularParticipanteAoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public ObservableCollection<dynamic> Participantes { get; } = new();
        public int ParticipanteId { get; set; }

        public ICommand AdicionarCommand { get; }

        public VincularParticipanteAoEventoViewModel(int eventoId)
        {
            _eventoId = eventoId;
            AdicionarCommand = new RelayCommand(async _ => await AdicionarAsync());
            _ = CarregarAsync();   // <<< importante: NÃO atribuir a Title
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();
            var lista = await db.Participantes
                .AsNoTracking()
                .OrderBy(p => p.NomeCompleto)
                .Select(p => new { p.Id, p.NomeCompleto })
                .ToListAsync();

            Participantes.Clear();
            foreach (var p in lista) Participantes.Add(p);
        }

        private async Task AdicionarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AdicionarParticipanteAsync(_eventoId, ParticipanteId);
                MessageBox.Show("Participante vinculado com sucesso!");
                Application.Current.Windows[0]?.Close();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}