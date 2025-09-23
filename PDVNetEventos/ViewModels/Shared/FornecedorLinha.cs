using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.ViewModels.Shared
{
    public class FornecedorLinha
    {
        public int Id { get; set; }
        public string NomeServico { get; set; } = "";
        public string CNPJ { get; set; } = "";
        public decimal? PrecoPadrao { get; set; }
        public decimal ValorAcordado { get; set; }
    }
}
