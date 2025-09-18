using PDVNetEventos.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.Data.Entities
{
    public class Fornecedor
    {
        public int Id { get; set; }
        public string NomeServico { get; set; } = "";
        public string CNPJ { get; set; } = ""; // único
        public decimal? PrecoPadrao { get; set; }

        public ICollection<EventoFornecedor> Eventos { get; set; } = new List<EventoFornecedor>();
    }
}
