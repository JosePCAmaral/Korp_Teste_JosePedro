namespace Faturamento.Api.Models
{
    public enum StatusNotaFiscal
    {
        Aberta,
        Fechada
    }

    public class NotaFiscal
    {
        public int Id { get; set; }
        public int Numeracao { get; set; }
        public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public List<ItemNotaFiscal> Itens { get; set; } = new();
    }
}
