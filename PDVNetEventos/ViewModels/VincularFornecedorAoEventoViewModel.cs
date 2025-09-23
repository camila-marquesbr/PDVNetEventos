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
using PDVNetEventos.Services;

namespace PDVNetEventos.ViewModels
{
    public class VincularFornecedorAoEventoViewModel : INotifyPropertyChanged
    {
        private readonly int _eventoId;

        public ObservableCollection<dynamic> Fornecedores { get; } = new();
        public int FornecedorId { get; set; }
        public decimal ValorAcordado { get; set; }

        public ICommand AdicionarCommand { get; }

        public VincularFornecedorAoEventoViewModel(int eventoId)
        {
            _eventoId = eventoId;
            AdicionarCommand = new RelayCommand(async _ => await AdicionarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            using var db = new AppDbContext();
            var lista = await db.Fornecedores.AsNoTracking()
                         .OrderBy(f => f.NomeServico)
                         .Select(f => new { f.Id, f.NomeServico })
                         .ToListAsync();

            Fornecedores.Clear();
            foreach (var f in lista) Fornecedores.Add(f);
        }

        private async Task AdicionarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AdicionarFornecedorAsync(_eventoId, FornecedorId, ValorAcordado);
                MessageBox.Show("Fornecedor vinculado com sucesso!");
                Application.Current.Windows[0]?.Close(); // feche a janela atual se preferir
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnChanged(string n) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}