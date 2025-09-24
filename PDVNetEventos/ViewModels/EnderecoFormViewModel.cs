using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using PDVNetEventos.Entities;
using PDVNetEventos.Services.Cep;
using PDVNetEventos.Commands;

namespace PDVNetEventos.ViewModels
{
    public class EnderecoFormViewModel : INotifyPropertyChanged
    {
        private readonly ICepService _cepService;
        private CancellationTokenSource? _cts;

        public EnderecoFormViewModel(ICepService cepService)
        {
            _cepService = cepService;
            BuscarCepCommand = new AsyncRelayCommand(BuscarCepAsync, CanBuscarCep);
        }

        private string _cep = "";
        public string Cep
        {
            get => _cep;
            set { _cep = value; OnPropertyChanged(); ((AsyncRelayCommand)BuscarCepCommand).RaiseCanExecuteChanged(); }
        }

        private string _logradouro = "";
        public string Logradouro { get => _logradouro; set { _logradouro = value; OnPropertyChanged(); } }

        private string _complemento = "";
        public string Complemento { get => _complemento; set { _complemento = value; OnPropertyChanged(); } }

        private string _bairro = "";
        public string Bairro { get => _bairro; set { _bairro = value; OnPropertyChanged(); } }

        private string _localidade = "";
        public string Localidade { get => _localidade; set { _localidade = value; OnPropertyChanged(); } }

        private string _uf = "";
        public string Uf { get => _uf; set { _uf = value; OnPropertyChanged(); } }

        private string _status = "";
        public string Status { get => _status; set { _status = value; OnPropertyChanged(); } }

        public ICommand BuscarCepCommand { get; }

        private bool CanBuscarCep() => !string.IsNullOrWhiteSpace(Cep);

        private async Task BuscarCepAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            Status = "Consultando CEP...";
            var address = await _cepService.GetByCepAsync(Cep, _cts.Token);
            if (address is null)
            {
                Status = "CEP não encontrado ou inválido.";
                return;
            }

            Preencher(address);
            Status = "Endereço carregado.";
        }

        private void Preencher(Address a)
        {
            // Evita sobrescrever complemento digitado manualmente
            Logradouro = a.Logradouro;
            Bairro = a.Bairro;
            Localidade = a.Localidade;
            Uf = a.Uf;
            // Cep formatado opcional
            Cep = FormatarCep(a.Cep);
        }

        private static string FormatarCep(string cep)
        {
            var digits = System.Text.RegularExpressions.Regex.Replace(cep ?? "", "[^0-9]", "");
            return digits.Length == 8 ? $"{digits[..5]}-{digits[5..]}" : cep ?? "";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}