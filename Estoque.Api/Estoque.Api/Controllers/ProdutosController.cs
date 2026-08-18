namespace Estoque.Api.Controllers
{
    using Estoque.Api.Data;
    using Estoque.Api.Models;
    using Estoque.Api.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly EstoqueDbContext _context;
        private readonly OllamaClient _ollamaClient;

        public ProdutosController(EstoqueDbContext context, OllamaClient ollamaClient)
        {
            _context = context;
            _ollamaClient = ollamaClient;
        }

        [HttpGet("sugerir-codigo")]
        public async Task<IActionResult> SugerirCodigo([FromQuery] string descricao)
        {
            if (string.IsNullOrWhiteSpace(descricao))
                return BadRequest("Informe uma descrição para gerar o código.");

            var codigo = await _ollamaClient.SugerirCodigoAsync(descricao);
            return Ok(new { codigo });
        }

        [HttpGet]
        public async Task<ActionResult<List<Produto>>> GetAll()
        {
            return await _context.Produtos.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetById(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto is null) return NotFound();
            return produto;
        }

        [HttpPost]
        public async Task<ActionResult<Produto>> Create(ProdutoCreateDto dto)
        {
            var produto = new Produto
            {
                Codigo = dto.Codigo,
                Descricao = dto.Descricao,
                Saldo = dto.Saldo
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = produto.Id }, produto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProdutoUpdateDto dto)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto is null) return NotFound();

            produto.Codigo = dto.Codigo;
            produto.Descricao = dto.Descricao;
            produto.Saldo = dto.Saldo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // Endpoint interno: usado pelo Faturamento.Api na hora de imprimir a nota
        [HttpPost("{id}/baixa")]
        public async Task<IActionResult> BaixarSaldo(int id, [FromBody] int quantidade)
        {
            var produto = await _context.Produtos.FindAsync(id);
            if (produto is null) return NotFound($"Produto {id} não encontrado.");

            if (produto.Saldo < quantidade)
                return BadRequest($"Saldo insuficiente para o produto {produto.Codigo}. Saldo atual: {produto.Saldo}.");

            produto.Saldo -= quantidade;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict($"O produto {produto.Codigo} foi alterado por outra operação simultânea. Tente novamente.");
            }

            return Ok(produto);
        }
    }
}
