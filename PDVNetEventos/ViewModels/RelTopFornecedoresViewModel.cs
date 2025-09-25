using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using PDVNetEventos.Commands;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class RelTopFornecedoresViewModel : INotifyPropertyChanged
    {
        private readonly RelatoriosService _svc = new();
        public ObservableCollection<TopFornecedorLinha> Itens { get; } = new();
        public ICommand RecarregarCommand { get; }

        public RelTopFornecedoresViewModel()
        {
            RecarregarCommand = new RelayCommand(async _ => await CarregarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            var dados = await _svc.ObterFornecedoresMaisUsadosAsync();
            Itens.Clear();
            foreach (var x in dados) Itens.Add(x);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}