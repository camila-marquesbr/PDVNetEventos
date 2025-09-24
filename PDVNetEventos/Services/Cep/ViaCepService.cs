using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using PDVNetEventos.Entities;

namespace PDVNetEventos.Services.Cep
{
    public class ViaCepService : ICepService
    {
        private static readonly Regex DigitsOnly = new("[^0-9]", RegexOptions.Compiled);
        private readonly HttpClient _http;

        public ViaCepService(HttpClient httpClient)
        {
            _http = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            if (_http.BaseAddress == null)
                _http.BaseAddress = new Uri("https://viacep.com.br/");
        }

        public async Task<Address?> GetByCepAsync(string cep, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(cep)) return null;

            // Normaliza CEP: só dígitos
            var digits = DigitsOnly.Replace(cep, "");
            if (digits.Length != 8) return null;

            // ViaCEP: /ws/{cep}/json/
            var url = $"ws/{digits}/json/";
            var dto = await _http.GetFromJsonAsync<ViaCepResponse>(url, cancellationToken);
            if (dto == null || dto.Erro) return null;

            return new Address
            {
                Cep = dto.Cep ?? digits,
                Logradouro = dto.Logradouro ?? "",
                Complemento = dto.Complemento ?? "",
                Bairro = dto.Bairro ?? "",
                Localidade = dto.Localidade ?? "",
                Uf = dto.Uf ?? "",
                Ibge = dto.Ibge ?? "",
                Gia = dto.Gia ?? "",
                Ddd = dto.Ddd ?? "",
                Siafi = dto.Siafi ?? ""
            };
        }

        private sealed class ViaCepResponse
        {
            [JsonPropertyName("cep")] public string? Cep { get; set; }
            [JsonPropertyName("logradouro")] public string? Logradouro { get; set; }
            [JsonPropertyName("complemento")] public string? Complemento { get; set; }
            [JsonPropertyName("bairro")] public string? Bairro { get; set; }
            [JsonPropertyName("localidade")] public string? Localidade { get; set; }
            [JsonPropertyName("uf")] public string? Uf { get; set; }
            [JsonPropertyName("ibge")] public string? Ibge { get; set; }
            [JsonPropertyName("gia")] public string? Gia { get; set; }
            [JsonPropertyName("ddd")] public string? Ddd { get; set; }
            [JsonPropertyName("siafi")] public string? Siafi { get; set; }
            [JsonPropertyName("erro")] public bool Erro { get; set; }
        }
    }
}