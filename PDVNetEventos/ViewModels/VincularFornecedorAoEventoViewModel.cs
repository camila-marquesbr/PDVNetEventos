using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using PDVNetEventos.Commands;
using PDVNetEventos.Data;
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class VincularFornecedorAoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public ObservableCollection<FornecedorGeralLinha> Itens { get; } = new();
        public int FornecedorId { get; set; }
        public decimal ValorAcordado { get; set; }

        public ICommand AtualizarCommand { get; }
        public ICommand AdicionarCommand { get; }

        public VincularFornecedorAoEventoViewModel(int eventoId)
        {
            _eventoId = eventoId;
            AtualizarCommand = new RelayCommand(async _ => await CarregarAsync());
            AdicionarCommand = new RelayCommand(async _ => await AdicionarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();

            var lista = await db.Fornecedores.AsNoTracking()
                .Select(f => new FornecedorGeralLinha
                {
                    Id = f.Id,
                    NomeServico = f.NomeServico,
                    CNPJ = f.CNPJ,
                    PrecoPadrao = f.PrecoPadrao
                })
                .OrderBy(f => f.NomeServico)
                .ToListAsync();

            Itens.Clear();
            foreach (var f in lista) Itens.Add(f);
        }

        private async Task AdicionarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AdicionarFornecedorAsync(_eventoId, FornecedorId, ValorAcordado);
                MessageBox.Show("Fornecedor vinculado com sucesso!");
                CloseWindow();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void CloseWindow()
            => Application.Current.Windows.Cast<Window>()
                 .FirstOrDefault(w => w.DataContext == this)?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}