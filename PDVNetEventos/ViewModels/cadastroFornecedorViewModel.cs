using System;
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
using PDVNetEventos.Services.Cep;

namespace PDVNetEventos.ViewModels
{
    public class cadastroFornecedorViewModel : INotifyPropertyChanged
    {
        private string _nomeServico = "";
        private string _cnpj = "";
        private decimal? _precoPadrao;

        // para vincular fornecedor a um evento
        public ObservableCollection<Evento> Eventos { get; } = new();
        private int _eventoId;
        private decimal _valorAcordado;

        public int EventoId { get => _eventoId; set { _eventoId = value; OnPropertyChanged(nameof(EventoId)); } }
        public decimal ValorAcordado { get => _valorAcordado; set { _valorAcordado = value; OnPropertyChanged(nameof(ValorAcordado)); } }

        public string NomeServico { get => _nomeServico; set { _nomeServico = value; OnPropertyChanged(nameof(NomeServico)); } }
        public string CNPJ { get => _cnpj; set { _cnpj = value; OnPropertyChanged(nameof(CNPJ)); } }
        public decimal? PrecoPadrao { get => _precoPadrao; set { _precoPadrao = value; OnPropertyChanged(nameof(PrecoPadrao)); } }

        public ICommand SalvarCommand { get; }
        public ICommand VincularAoEventoCommand { get; }

        public cadastroFornecedorViewModel()
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
                int id = await svc.CriarFornecedorAsync(NomeServico, CNPJ, PrecoPadrao);
                System.Windows.MessageBox.Show($"Fornecedor salvo! Id={id}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private async void VincularAoEvento()
        {
            try
            {
                using var db = new AppDbContext();
                var f = await db.Fornecedores.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
                if (f == null) { System.Windows.MessageBox.Show("Salve um fornecedor primeiro."); return; }

                var svc = new EventService();
                await svc.AdicionarFornecedorAsync(EventoId, f.Id, ValorAcordado);

                System.Windows.MessageBox.Show($"Fornecedor '{f.NomeServico}' vinculado ao evento.");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erro: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}