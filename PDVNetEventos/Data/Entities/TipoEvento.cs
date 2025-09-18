using PDVNetEventos.Data.Entities;
using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.Data.Entities
{
    public class TipoEvento
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = "";

        public ICollection<Evento> Eventos { get; set; } = new List<Evento>();
    }
}
