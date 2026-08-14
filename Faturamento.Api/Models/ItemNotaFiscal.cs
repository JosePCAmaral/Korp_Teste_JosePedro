namespace Faturamento.Api.Models
{
    using System.Text.Json.Serialization;

    public class ItemNotaFiscal
    {
        public int Id { get; set; }
        public int NotaFiscalId { get; set; }

        [JsonIgnore]
        public NotaFiscal? NotaFiscal { get; set; }

        public int ProdutoId { get; set; }
        public string ProdutoCodigo { get; set; } = string.Empty;
        public string ProdutoDescricao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
    }
}