using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.Services.Auth
{
    public interface IAuthService
    {
        /// <summary>Valida credenciais e retorna um "token" mock se ok.</summary>
        string? Login(string username, string password);

        /// <summary>Usuário atual (memoriza após login).</summary>
        string? CurrentUser { get; }
        string? CurrentToken { get; }
        void Logout();
    }
}
