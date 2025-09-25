using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;

namespace PDVNetEventos.ViewModels
{
    public class EventoResumo
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Periodo { get; set; } = "";
        public decimal Orcamento { get; set; }
        public decimal Gasto { get; set; }
        public decimal Saldo { get; set; }
    }

    public class ListarEventosViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<EventoResumo> Itens { get; } = new();
        public ICommand RecarregarCommand { get; }

        public ListarEventosViewModel()
        {
            RecarregarCommand = new RelayCommand(async _ => await CarregarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var lista = await db.Eventos
                .Select(e => new EventoResumo
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    Periodo = e.DataInicio.ToString("dd/MM/yyyy") + " - " + e.DataFim.ToString("dd/MM/yyyy"),
                    Orcamento = e.OrcamentoMaximo,
                    Gasto = db.EventosFornecedores          
                               .Where(v => v.EventoId == e.Id)
                               .Sum(v => (decimal?)v.ValorAcordado) ?? 0m
                })
                .ToListAsync();

            foreach (var item in lista)
                item.Saldo = item.Orcamento - item.Gasto;

            Itens.Clear();
            foreach (var i in lista) Itens.Add(i);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}