namespace Estoque.Api.Models
{
    using System.ComponentModel.DataAnnotations;

    public class ProdutoCreateDto
    {
        [Required(ErrorMessage = "O código é obrigatório.")]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
        public int Saldo { get; set; }
    }

    public class ProdutoUpdateDto
    {
        [Required(ErrorMessage = "O código é obrigatório.")]
        [StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        [Range(0, int.MaxValue, ErrorMessage = "O saldo não pode ser negativo.")]
        public int Saldo { get; set; }
    }
}
