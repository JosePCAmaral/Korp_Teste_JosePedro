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
        var prompt = $@"Você é um sistema de cadastro de estoque de um supermercado. Sua única tarefa é sugerir um NOME de produto real e comum (não uma frase de marketing, não uma descrição de sistema), compatível com o código informado.

        Exemplos:
        Código: PROD-001 -> Nome: Caixa de Leite Integral 1L
        Código: ARZ-5KG -> Nome: Pacote de Arroz Branco 5kg
        Código: SAB-BAR -> Nome: Sabonete em Barra 90g

        Código: {codigo} -> Nome:";

        var payload = new
        {
            model = Modelo,
            prompt,
            stream = false,
            options = new
            {
                temperature = 0.4,
                num_predict = 20,
                stop = new[] { "\n" }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("/api/generate", payload);
        response.EnsureSuccessStatusCode();

        var resultado = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
        var texto = resultado?.Response.Trim() ?? string.Empty;

        // Alguns modelos pequenos ecoam "Nome:" de volta — remove se aparecer
        if (texto.StartsWith("Nome:", StringComparison.OrdinalIgnoreCase))
            texto = texto["Nome:".Length..].Trim();

        return texto.Trim('"', '.', ' ');
    }
}