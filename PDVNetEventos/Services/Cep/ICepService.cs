using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PDVNetEventos.Entities;

namespace PDVNetEventos.Services.Cep
{
    public interface ICepService
    {
        /// <summary>Consulta CEP no provedor configurado.</summary>
        /// <param name="cep">Apenas dígitos (8) ou com máscara (será limpo).</param>
        Task<Address?> GetByCepAsync(string cep, CancellationToken cancellationToken = default);
    }
}
