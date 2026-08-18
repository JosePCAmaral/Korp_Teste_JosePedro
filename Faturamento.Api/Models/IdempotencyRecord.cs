using System.ComponentModel.DataAnnotations;

namespace Faturamento.Api.Models;

public class IdempotencyRecord
{
    [Key]
    public string Chave { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string RespostaJson { get; set; } = string.Empty;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
}