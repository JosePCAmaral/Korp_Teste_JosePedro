namespace Faturamento.Api.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ItemNotaFiscalCreateDto
    {
        [Required]
        public int ProdutoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
        public int Quantidade { get; set; }
    }

    public class NotaFiscalCreateDto
    {
        [MinLength(1, ErrorMessage = "A nota precisa de pelo menos um item.")]
        public List<ItemNotaFiscalCreateDto> Itens { get; set; } = new();
    }
}
