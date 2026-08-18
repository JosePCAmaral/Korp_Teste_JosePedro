using System.Text.Json.Serialization;

namespace Estoque.Api.Services;

public class OllamaGenerateResponse
{
    [JsonPropertyName("response")]
    public string Response { get; set; } = string.Empty;
}

public class OllamaClient
{
    private readonly HttpClient _httpClient;
    private const string Modelo = "llama3.2:1b";

    public OllamaClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Ollama");
    }

    public async Task<string> SugerirDescricaoAsync(string codigo)
    {
        var prompt = $"Gere uma descrição curta e objetiva (máximo 8 palavras) para um produto de estoque com o código '{codigo}'. Responda apenas com a descrição, sem explicações, aspas ou pontuação extra.";

        var payload = new { model = Modelo, prompt, stream = false };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", payload);
        response.EnsureSuccessStatusCode();

        var resultado = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        return resultado?.Response.Trim() ?? string.Empty;
    }
}