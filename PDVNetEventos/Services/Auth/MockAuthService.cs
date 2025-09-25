using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PDVNetEventos.Services.Auth
{
    public class MockAuthService : IAuthService
    {
        public string? CurrentUser { get; private set; }
        public string? CurrentToken { get; private set; }

        // Usuários mock (troque à vontade)
        private static readonly (string user, string pass)[] _users =
        {
            ("admin", "123"),
            ("user",  "123")
        };

        public string? Login(string username, string password)
        {
            foreach (var (user, pass) in _users)
            {
                if (string.Equals(user, username, StringComparison.OrdinalIgnoreCase) && pass == password)
                {
                    CurrentUser = user;
                    CurrentToken = "mock-jwt-" + Guid.NewGuid().ToString("N");
                    return CurrentToken;
                }
            }
            return null;
        }

        public void Logout()
        {
            CurrentUser = null;
            CurrentToken = null;
        }
    }
}
