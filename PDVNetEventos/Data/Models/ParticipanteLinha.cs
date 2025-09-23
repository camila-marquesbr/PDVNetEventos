using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PDVNetEventos.Data.Entities;

namespace PDVNetEventos.Models
{
    public class ParticipanteLinha
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = "";
        public string CPF { get; set; } = "";
        public string Tipo { get; set; } = "";   // <- string
    }
}