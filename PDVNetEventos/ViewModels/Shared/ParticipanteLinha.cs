using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.ViewModels
{
    /// <summary>
    /// DTO simples para exibir participante em listas/grids.
    /// Use em todas as telas que renderizam participantes.
    /// </summary>
    public class ParticipanteLinha
    {
        public int Id { get; set; }
        public string NomeCompleto { get; set; } = "";
        public string CPF { get; set; } = "";
        public string Tipo { get; set; } = ""; // "VIP", "Interno", "Externo"
    }
}