using System;
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
    public class EditarEventoViewModel : INotifyPropertyChanged
    {
        public int Id { get; }
        private string _nome = "";
        public string Nome { get => _nome; set { _nome = value; OnPropertyChanged(nameof(Nome)); } }

        private DateTime _inicio = DateTime.Today, _fim = DateTime.Today;
        public DateTime DataInicio { get => _inicio; set { _inicio = value; OnPropertyChanged(nameof(DataInicio)); } }
        public DateTime DataFim { get => _fim; set { _fim = value; OnPropertyChanged(nameof(DataFim)); } }

        private int _capacidade;
        public int Capacidade { get => _capacidade; set { _capacidade = value; OnPropertyChanged(nameof(Capacidade)); } }

        private decimal _orcamento;
        public decimal OrcamentoMaximo { get => _orcamento; set { _orcamento = value; OnPropertyChanged(nameof(OrcamentoMaximo)); } }

        public ICommand SalvarCommand { get; }
        public ICommand CancelarCommand { get; }

        public EditarEventoViewModel(int id)
        {
            Id = id;
            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            CancelarCommand = new RelayCommand(_ => Close());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            try
            {
                using var db = new AppDbContext();
                var ev = await db.Eventos.AsNoTracking().FirstAsync(x => x.Id == Id);
                Nome = ev.Nome;
                DataInicio = ev.DataInicio;
                DataFim = ev.DataFim;
                Capacidade = ev.CapacidadeMaxima;
                OrcamentoMaximo = ev.OrcamentoMaximo;
            }
            catch (Exception ex) { MessageBox.Show("Erro ao carregar: " + ex.Message); }
        }

        private async Task SalvarAsync()
        {
            try
            {
                var ev = new Evento
                {
                    Id = Id,
                    Nome = Nome,
                    DataInicio = DataInicio,
                    DataFim = DataFim,
                    CapacidadeMaxima = Capacidade,
                    OrcamentoMaximo = OrcamentoMaximo
                };

                var svc = new EventService();
                await svc.AtualizarEventoAsync(ev);
                MessageBox.Show("Evento atualizado!");
                Close();
            }
            catch (Exception ex) { MessageBox.Show("Erro ao salvar: " + ex.Message); }
        }

        private void Close() =>
            Application.Current.Windows[Application.Current.Windows.Count - 1]?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}