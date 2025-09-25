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
    public class EditarParticipanteViewModel : INotifyPropertyChanged
    {
        private readonly int _id;

        public string Nome { get; set; } = "";
        public string CPF { get; set; } = "";
        public string? Telefone { get; set; }
        public TipoParticipante Tipo { get; set; }

        public ICommand SalvarCommand { get; }

        public EditarParticipanteViewModel(int id)
        {
            _id = id;
            SalvarCommand = new RelayCommand(async _ => await SalvarAsync());
            _ = CarregarAsync();
        }

        private async Task CarregarAsync()
        {
            var svc = new EventService();
            var p = await svc.ObterParticipanteAsync(_id);

            Nome = p.NomeCompleto;
            CPF = p.CPF;
            Telefone = p.Telefone;
            Tipo = p.Tipo;

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }

        private async Task SalvarAsync()
        {
            try
            {
                var svc = new EventService();
                await svc.AtualizarParticipanteAsync(new Participante
                {
                    Id = _id,
                    NomeCompleto = Nome,
                    CPF = CPF,
                    Telefone = Telefone,
                    Tipo = Tipo
                });

                MessageBox.Show("Participante atualizado!");
                Fechar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro: " + ex.Message);
            }
        }

        private void Fechar()
            => System.Windows.Application.Current.Windows[0]?.Close();

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}