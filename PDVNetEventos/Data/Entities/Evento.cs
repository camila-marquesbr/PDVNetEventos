using PDVNetEventos.Data.Entities;
using System;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace PDVNetEventos.Data.Entities
{
    public class Evento
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }

        // Endereço
        public string? Rua { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? Observacoes { get; set; }

        // Endereço (preenchido via CEP)
        public string? Cep { get; set; }
        public string? Logradouro { get; set; }
        public string? Complemento { get; set; }
        public string? Bairro { get; set; }
        public string? Localidade { get; set; } 
        public string? Uf { get; set; }

        public int CapacidadeMaxima { get; set; }
        public decimal OrcamentoMaximo { get; set; }

        public int TipoEventoId { get; set; }
        public TipoEvento? TipoEvento { get; set; }

        public ICollection<EventoFornecedor> Fornecedores { get; set; } = new List<EventoFornecedor>();
        public ICollection<EventoParticipante> Participantes { get; set; } = new List<EventoParticipante>();
    }
}