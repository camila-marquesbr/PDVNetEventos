using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;
using System.Collections.ObjectModel;

namespace PDVNetEventos.ViewModels
{
    public class cadastroParticipanteViewModel : INotifyPropertyChanged
    {
        // campos do form
        private string _nome = "";
        private string _cpf = "";
        private string? _telefone = "";
        private TipoParticipante _tipo = TipoParticipante.Externo;

        // para vincular participante a um evento
        public ObservableCollection<Evento> Eventos { get; } = new();
        private int _eventoId;
        public int EventoId { get => _eventoId; set { _eventoId = value; OnPropertyChanged(nameof(EventoId)); } }

        // binds
        public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(nameof(Nome)); } }
        public string CPF { get => _cpf; set { _cpf = value; OnPropertyChanged(nameof(CPF)); } }
        public string? Telefone { get => _telefone; set { _telefone = value; OnPropertyChanged(nameof(Telefone)); } }
        public TipoParticipante Tipo { get => _tipo; set { _tipo = value; OnPropertyChanged(nameof(Tipo)); } }

        // comandos
        public ICommand SalvarCommand { get; }
        public ICommand VincularAoEventoCommand { get; }

        public cadastroParticipanteViewModel()
        {
            SalvarCommand = new RelayCommand(_ => Salvar());
            VincularAoEventoCommand = new RelayCommand(_ => VincularAoEvento());
            _ = CarregarEventosAsync();
        }

        private async Task CarregarEventosAsync()
        {
            using var db = new AppDbContext();
            var lista = await db.Eventos.AsNoTracking().OrderBy(e => e.Nome).ToListAsync();
            Eventos.Clear();
            foreach (var e in lista) Eventos.Add(e);
            if (Eventos.Count > 0) EventoId = Eventos[0].Id;
        }

        private async void Salvar()
        {
            try
            {
                var svc = new EventService();
                int id = await svc.CreateParticipantAsync(Nome, CPF, Telefone, Tipo);
                System.Windows.MessageBox.Show($"Participante salvo! Id={id}");
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private async void VincularAoEvento()
        {
            try
            {
                using var db = new AppDbContext();
                var p = await db.Participantes.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                if (p == null) { System.Windows.MessageBox.Show("Salve um participante primeiro."); return; }

                var svc = new EventService();
                await svc.AddParticipantToEventAsync(EventoId, p.Id);

                System.Windows.MessageBox.Show($"Participante '{p.NomeCompleto}' vinculado ao evento.");
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show("Erro: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
