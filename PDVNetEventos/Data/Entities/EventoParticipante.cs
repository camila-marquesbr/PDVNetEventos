using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace PDVNetEventos.Data.Entities
{
    public class EventoParticipante
    {
        public int EventoId { get; set; }
        public Evento? Evento { get; set; }

        public int ParticipanteId { get; set; }
        public Participante? Participante { get; set; }
    }
}
