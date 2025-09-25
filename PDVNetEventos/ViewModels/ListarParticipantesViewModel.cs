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
using PDVNetEventos.ViewModels.Shared;

namespace PDVNetEventos.ViewModels
{
    public class ListarParticipantesViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ParticipanteLinha> Itens { get; } = new();

        public ICommand AtualizarCommand { get; }
        public ICommand EditarCommand { get; }
        public ICommand ExcluirCommand { get; }

        public ListarParticipantesViewModel()
        {
            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            EditarCommand = new RelayCommand(p => Editar((ParticipanteLinha)p!));
            ExcluirCommand = new RelayCommand(async p => await ExcluirAsync((ParticipanteLinha)p!));
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            try
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
                        Tipo = p.Tipo
                    })
                    .ToListAsync();

                Itens.Clear();
                foreach (var i in lista) Itens.Add(i);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao carregar: " + ex.Message);
            }
        }

        private void Editar(ParticipanteLinha p)
        {
            new PDVNetEventos.Views.EditarParticipante(p.Id).ShowDialog();
            _ = CarregarAsync();
        }

        private async Task ExcluirAsync(ParticipanteLinha p)
        {
            if (MessageBox.Show($"Excluir participante '{p.NomeCompleto}'?", "Confirmação",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;

            try
            {
                using var db = new AppDbContext();


                var v1 = db.EventosParticipantes.Where(x => x.ParticipanteId == p.Id);
                db.RemoveRange(v1);

                var ent = await db.Participantes.FindAsync(p.Id);
                if (ent != null) db.Participantes.Remove(ent);

                await db.SaveChangesAsync();
                Itens.Remove(p);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro ao excluir: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}