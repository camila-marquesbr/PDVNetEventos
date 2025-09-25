using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using PDVNetEventos.Commands;
using PDVNetEventos.Data.Entities;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class EditarEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _id;

        public string NomeEvento { get; set; } = "";
        public DateTime DataInicio { get; set; } = DateTime.Today;
        public DateTime DataFim { get; set; } = DateTime.Today;
        public int Capacidade { get; set; }
        public decimal Orcamento { get; set; }
        public int TipoEventoId { get; set; }

        public ICommand SalvarCommand { get; }

        public EditarEventoViewModel(int id)
        {
            _id = id;
            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            var svc = new EventService();
            var e = await svc.ObterEventoAsync(_id);

            NomeEvento = e.Nome;
            DataInicio = e.DataInicio;
            DataFim = e.DataFim;
            Capacidade = e.CapacidadeMaxima;
            Orcamento = e.OrcamentoMaximo;
            TipoEventoId = e.TipoEventoId;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private async Task SalvarAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NomeEvento))
                {
                    MessageBox.Show("Informe o nome do evento."); return;
                }
                if (DataInicio > DataFim)
                {
                    MessageBox.Show("Data início não pode ser após a data fim."); return;
                }
                if (Capacidade <= 0)
                {
                    MessageBox.Show("Capacidade deve ser > 0."); return;
                }

                var svc = new EventService();
                await svc.AtualizarEventoAsync(new Evento
                {
                    Id = _id,
                    Nome = NomeEvento,
                    DataInicio = DataInicio,
                    DataFim = DataFim,
                    CapacidadeMaxima = Capacidade,
                    OrcamentoMaximo = Orcamento,
                    TipoEventoId = TipoEventoId
                });

                MessageBox.Show("Evento atualizado!");
                Fechar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void Fechar()
            => Application.Current.Windows[0]?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}