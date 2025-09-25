using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.ViewModels.Shared
{
    public class ParticipanteLinha
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = "";
        public string CPF { get; set; } = "";
        public PDVNetEventos.Data.Entities.TipoParticipante Tipo { get; set; }
    }
}