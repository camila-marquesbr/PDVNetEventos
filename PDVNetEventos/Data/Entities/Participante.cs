using PDVNetEventos.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.Data.Entities
{
    public class Participante
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = "";
        public string CPF { get; set; } = ""; 
        public string? Telefone { get; set; }
        public TipoParticipante Tipo { get; set; }

        public ICollection<EventoParticipante> Eventos { get; set; } = new List<EventoParticipante>();
    }
}
