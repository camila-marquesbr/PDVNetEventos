using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

        public ObservableCollection<ParticipanteLinha> Participantes { get; } = new();

        private int _participanteId;
        public int ParticipanteId
        {
            get => _participanteId;
            set { _participanteId = value; OnPropertyChanged(); }
        }

        public ICommand AdicionarCommand { get; }

        public VincularParticipanteAoEventoViewModel(int eventoId)
        {
            _eventoId = eventoId;

            AdicionarCommand = new RelayCommand(async _ => await AdicionarAsync());

            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var lista = await db.Participantes
                .AsNoTracking()
                .OrderBy(p => p.NomeCompleto)
                .Select(p => new ParticipanteLinha
                {
                    Id = p.Id,
                    NomeCompleto = p.NomeCompleto,
                    CPF = p.CPF,
                    Tipo = p.Tipo.ToString()
                })
                .ToListAsync();

            Participantes.Clear();
            foreach (var p in lista)
                Participantes.Add(p);
        }

        private async Task AdicionarAsync()
        {
            try
            {
                if (ParticipanteId <= 0)
                {
                    MessageBox.Show("Selecione um participante.");
                    return;
                }

                var svc = new EventService();
                await svc.AdicionarParticipanteAsync(_eventoId, ParticipanteId);

                MessageBox.Show("Participante vinculado com sucesso!");
                CloseWindow();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void CloseWindow()
        {
            Application.Current.Windows
                .OfType<Window>()
                .FirstOrDefault(w => w.DataContext == this)
                ?.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? n = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}