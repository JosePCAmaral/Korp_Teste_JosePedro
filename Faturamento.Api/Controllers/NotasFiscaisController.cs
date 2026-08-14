namespace Faturamento.Api.Controllers
{
    using Faturamento.Api.Data;
    using Faturamento.Api.Models;
    using Faturamento.Api.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/[controller]")]
    public class NotasFiscaisController : ControllerBase
    {
        private readonly FaturamentoDbContext _context;
        private readonly EstoqueApiClient _estoqueApi;

        public NotasFiscaisController(FaturamentoDbContext context, EstoqueApiClient estoqueApi)
        {
            _context = context;
            _estoqueApi = estoqueApi;
        }

        [HttpGet]
        public async Task<ActionResult<List<NotaFiscal>>> GetAll()
        {
            return await _context.NotasFiscais.Include(n => n.Itens).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<NotaFiscal>> GetById(int id)
        {
            var nota = await _context.NotasFiscais.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id);
            if (nota is null) return NotFound();
            return nota;
        }

        [HttpPost]
        public async Task<ActionResult<NotaFiscal>> Create(NotaFiscalCreateDto dto)
        {
            if (dto.Itens.Count == 0)
                return BadRequest("A nota precisa de pelo menos um item.");

            var ultimaNumeracao = await _context.NotasFiscais.MaxAsync(n => (int?)n.Numeracao) ?? 0;

            var nota = new NotaFiscal
            {
                Numeracao = ultimaNumeracao + 1,
                Status = StatusNotaFiscal.Aberta
            };

            foreach (var item in dto.Itens)
            {
                var produto = await _estoqueApi.ObterProdutoAsync(item.ProdutoId);
                if (produto is null)
                    return BadRequest($"Produto {item.ProdutoId} não encontrado no Estoque.");

                nota.Itens.Add(new ItemNotaFiscal
                {
                    ProdutoId = produto.Id,
                    ProdutoCodigo = produto.Codigo,
                    ProdutoDescricao = produto.Descricao,
                    Quantidade = item.Quantidade
                });
            }

            _context.NotasFiscais.Add(nota);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = nota.Id }, nota);
        }

        [HttpPost("{id}/imprimir")]
        public async Task<IActionResult> Imprimir(int id)
        {
            var nota = await _context.NotasFiscais.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id);
            if (nota is null) return NotFound();

            if (nota.Status != StatusNotaFiscal.Aberta)
                return BadRequest("Somente notas com status 'Aberta' podem ser impressas.");

            try
            {
                // Fase 1: valida o saldo de TODOS os itens antes de baixar qualquer um
                foreach (var item in nota.Itens)
                {
                    var produto = await _estoqueApi.ObterProdutoAsync(item.ProdutoId);
                    if (produto is null)
                        return BadRequest($"Produto {item.ProdutoCodigo} não encontrado no Estoque.");

                    if (produto.Saldo < item.Quantidade)
                        return BadRequest($"Saldo insuficiente para {produto.Codigo}. Disponível: {produto.Saldo}, solicitado: {item.Quantidade}.");
                }

                // Fase 2: só baixa de fato depois que TODOS passaram na validação
                foreach (var item in nota.Itens)
                {
                    var (sucesso, erro) = await _estoqueApi.BaixarSaldoAsync(item.ProdutoId, item.Quantidade);
                    if (!sucesso)
                        return BadRequest($"Falha ao baixar saldo do produto {item.ProdutoCodigo}: {erro}");
                }
            }
            catch (HttpRequestException)
            {
                return StatusCode(503, "Serviço de Estoque está indisponível no momento. A nota continua 'Aberta' — tente imprimir novamente em instantes.");
            }
            catch (TaskCanceledException)
            {
                return StatusCode(503, "Tempo de resposta do Serviço de Estoque esgotado. A nota continua 'Aberta' — tente novamente.");
            }

            nota.Status = StatusNotaFiscal.Fechada;
            await _context.SaveChangesAsync();
            return Ok(nota);
        }
    }
}
