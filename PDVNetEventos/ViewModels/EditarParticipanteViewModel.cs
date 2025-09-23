using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class EditarParticipanteViewModel : INotifyPropertyChanged
    {
        public int Id { get; }
        private string _nome = "", _cpf = "", _telefone = "";
        public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(nameof(Nome)); } }
        public string CPF { get => _cpf; set { _cpf = value; OnPropertyChanged(nameof(CPF)); } }
        public string Telefone { get => _telefone; set { _telefone = value; OnPropertyChanged(nameof(Telefone)); } }
        public TipoParticipante Tipo { get; set; }

        public ICommand SalvarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarParticipanteViewModel(int id)
        {
            Id = id;
            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            CancelarCommand = new RelayCommand(_ => Close());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();
            var p = await db.Participantes.AsNoTracking().FirstAsync(x => x.Id == Id);
            Nome = p.NomeCompleto; CPF = p.CPF; Telefone = p.Telefone ?? "";
            Tipo = p.Tipo;
        }

        private async Task SalvarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AtualizarParticipanteAsync(new Participante
                {
                    Id = Id,
                    NomeCompleto = Nome,
                    CPF = CPF,
                    Telefone = Telefone,
                    Tipo = Tipo
                });
                MessageBox.Show("Participante atualizado!");
                Close();
            }
            catch (System.Exception ex) { MessageBox.Show("Erro: " + ex.Message); }
        }

        private void Close() =>
            Application.Current.Windows[Application.Current.Windows.Count - 1]?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}