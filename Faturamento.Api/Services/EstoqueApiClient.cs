namespace Faturamento.Api.Services
{
    using System.Net.Http.Json;

    public class ProdutoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Saldo { get; set; }
    }

    public class EstoqueApiClient
    {
        private readonly HttpClient _httpClient;

        public EstoqueApiClient(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("EstoqueApi");
        }

        public async Task<ProdutoDto?> ObterProdutoAsync(int produtoId)
        {
            var response = await _httpClient.GetAsync($"/api/produtos/{produtoId}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<ProdutoDto>();
        }

        public async Task<(bool sucesso, string? erro)> BaixarSaldoAsync(int produtoId, int quantidade)
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/produtos/{produtoId}/baixa", quantidade);
            if (response.IsSuccessStatusCode) return (true, null);

            var erro = await response.Content.ReadAsStringAsync();
            return (false, erro);
        }
    }
}
