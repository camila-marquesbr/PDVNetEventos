using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public class cadastroEventoViewModel : INotifyPropertyChanged
    {
        // ===== NOVO: bloco de CEP =====
        /// <summary>
        /// Sub-VM responsável por CEP e campos de endereço (Cep, Logradouro, Bairro, Localidade, Uf, etc.)
        /// </summary>
        public EnderecoFormViewModel? Endereco { get; }

        // ---- campos privados (SEUS)
        private string? _nomeEvento = string.Empty;
        private DateTime _dataInicio = DateTime.Today;
        private DateTime _dataFim = DateTime.Today;
        private int _capacidade;
        private decimal _orcamento;
        private int _tipoEventoId;

        // ---- binds (SEUS)
        public string? NomeEvento { get => _nomeEvento; set { _nomeEvento = value; OnPropertyChanged(nameof(NomeEvento)); } }
        public DateTime DataInicio { get => _dataInicio; set { _dataInicio = value; OnPropertyChanged(nameof(DataInicio)); } }
        public DateTime DataFim { get => _dataFim; set { _dataFim = value; OnPropertyChanged(nameof(DataFim)); } }
        public int Capacidade { get => _capacidade; set { _capacidade = value; OnPropertyChanged(nameof(Capacidade)); } }
        public decimal Orcamento { get => _orcamento; set { _orcamento = value; OnPropertyChanged(nameof(Orcamento)); } }

        public int TipoEventoId
        {
            get => _tipoEventoId;
            set { _tipoEventoId = value; OnPropertyChanged(nameof(TipoEventoId)); }
        }

        public ObservableCollection<TipoEvento> TiposEvento { get; } = new();

        public ICommand SalvarCommand { get; }

        // ===== CONSTRUTORES =====

        /// <summary>
        /// Construtor principal (runtime): receba o serviço de CEP e inicialize a sub-VM de endereço.
        /// </summary>
        public cadastroEventoViewModel(ICepService cepService)
        {
            if (cepService == null) throw new ArgumentNullException(nameof(cepService));
            Endereco = new EnderecoFormViewModel(cepService);
            SalvarCommand = new RelayCommand(_ => Salvar());
            Init();
        }

        /// <summary>
        /// Construtor sem parâmetro (Designer/preview). Evite usar em runtime.
        /// </summary>
        public cadastroEventoViewModel()
        {
            // Endereco permanece null no designer; os bindings do XAML devem tolerar isso.
            SalvarCommand = new RelayCommand(_ => Salvar());
            Init();
        }

        /// <summary>
        /// Inicializações comuns aos construtores.
        /// </summary>
        private void Init()
        {
            _ = CarregarTiposAsync();
        }

        // ===== MÉTODOS (SEUS) =====

        private async Task CarregarTiposAsync()
        {
            using var db = new AppDbContext();
            var lista = await db.TiposEvento.AsNoTracking().ToListAsync();
            TiposEvento.Clear();
            foreach (var t in lista) TiposEvento.Add(t);
            if (TiposEvento.Count > 0) TipoEventoId = TiposEvento[0].Id;
        }

        // *** deixe APENAS ESTE método Salvar ***
        private async void Salvar()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(NomeEvento))
                { System.Windows.MessageBox.Show("Informe o nome do evento."); return; }

                if (Capacidade <= 0)
                { System.Windows.MessageBox.Show("Capacidade deve ser > 0."); return; }

                if (DataInicio > DataFim)
                { System.Windows.MessageBox.Show("Data início não pode ser após a data fim."); return; }

                var service = new EventService();

                var evento = new Evento
                {
                    Nome = NomeEvento!,
                    DataInicio = DataInicio,
                    DataFim = DataFim,
                    CapacidadeMaxima = Capacidade,
                    OrcamentoMaximo = Orcamento,
                    TipoEventoId = TipoEventoId
                };

                // validação de data já existente
                await service.ValidarDatasEventoAsync(evento);

                using var db = new AppDbContext();
                db.Eventos.Add(evento);
                await db.SaveChangesAsync();

                System.Windows.MessageBox.Show($"Evento '{evento.Nome}' salvo! Id={evento.Id}");
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Erro: " + ex.Message);
            }
        }

        // ===== INotifyPropertyChanged =====
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}