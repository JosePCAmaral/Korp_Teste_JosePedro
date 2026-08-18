using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

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

    private static readonly Regex CodigoValido = new(@"^[A-Z0-9]{2,10}(-[A-Z0-9]{2,10})?$", RegexOptions.Compiled);

    private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
    {
        "de", "da", "do", "das", "dos", "em", "para", "com", "e", "a", "o", "as", "os"
    };

    public OllamaClient(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("Ollama");
    }

    public async Task<string> SugerirCodigoAsync(string descricao)
    {
        var prompt = $@"Você é um sistema de cadastro de estoque de um supermercado. Sua única tarefa é sugerir um CÓDIGO curto para um produto, a partir do nome dele. O código deve ter o formato: três letras maiúsculas de uma palavra-chave do nome, um hífen, e três letras maiúsculas de outra palavra-chave do nome. Responda APENAS o código, em maiúsculas, sem explicações, sem pontuação extra.

        Exemplos:
        Nome: Farinha de Trigo -> Código: FAR-TRI
        Nome: Caixa de Leite Integral 1L -> Código: LEI-INT
        Nome: Pacote de Arroz Branco 5kg -> Código: ARR-BRA
        Nome: Sabonete em Barra 90g -> Código: SAB-BAR

        Nome: {descricao} -> Código:";

        var payload = new
        {
            model = Modelo,
            prompt,
            stream = false,
            options = new
            {
                temperature = 0.2,
                num_predict = 10,
                stop = new[] { "\n" }
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/generate", payload);
            response.EnsureSuccessStatusCode();

            var resultado = await response.Content.ReadFromJsonAsync<OllamaGenerateResponse>();
            var texto = (resultado?.Response ?? string.Empty).Trim();

            if (texto.StartsWith("Código:", StringComparison.OrdinalIgnoreCase))
                texto = texto["Código:".Length..].Trim();

            texto = texto.Trim('"', '.', ' ').ToUpperInvariant();

            return CodigoValido.IsMatch(texto) ? texto : GerarCodigoFallback(descricao);
        }
        catch
        {
            // Ollama indisponível, resposta fora do formato ou qualquer outra falha:
            // cai no gerador determinístico em vez de quebrar a funcionalidade.
            return GerarCodigoFallback(descricao);
        }
    }

    private static string GerarCodigoFallback(string descricao)
    {
        var palavras = descricao
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(p => !StopWords.Contains(p))
            .ToList();

        if (palavras.Count == 0)
            palavras = descricao.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();

        string Abrevia(string palavra)
        {
            var letras = new string(palavra.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            return letras.PadRight(3, 'X')[..3];
        }

        if (palavras.Count >= 2)
            return $"{Abrevia(palavras[0])}-{Abrevia(palavras[1])}";

        if (palavras.Count == 1)
        {
            var letras = new string(palavras[0].Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            return letras.Length >= 6 ? $"{letras[..3]}-{letras[3..6]}" : letras.PadRight(6, 'X');
        }

        return "PROD-000";
    }
}