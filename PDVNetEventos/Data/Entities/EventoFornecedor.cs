using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PDVNetEventos.Data.Entities
{
    public class EventoFornecedor
    {
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        public int FornecedorId { get; set; }
        public Fornecedor? Fornecedor { get; set; }

        public decimal ValorAcordado { get; set; }
    }
}
