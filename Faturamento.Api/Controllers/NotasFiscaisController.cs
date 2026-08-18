namespace Faturamento.Api.Controllers;

using Faturamento.Api.Data;
using Faturamento.Api.Models;
using Faturamento.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

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
    public async Task<IActionResult> Imprimir(int id, [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            var registroExistente = await _context.IdempotencyRecords.FindAsync(idempotencyKey);
            if (registroExistente is not null)
            {
                var corpoCache = JsonSerializer.Deserialize<object>(registroExistente.RespostaJson);
                return StatusCode(registroExistente.StatusCode, corpoCache);
            }
        }

        var nota = await _context.NotasFiscais.Include(n => n.Itens).FirstOrDefaultAsync(n => n.Id == id);
        if (nota is null) return NotFound();

        if (nota.Status != StatusNotaFiscal.Aberta)
            return await FinalizarComRegistro(idempotencyKey, 400, "Somente notas com status 'Aberta' podem ser impressas.");

        try
        {
            foreach (var item in nota.Itens)
            {
                var produto = await _estoqueApi.ObterProdutoAsync(item.ProdutoId);
                if (produto is null)
                    return await FinalizarComRegistro(idempotencyKey, 400, $"Produto {item.ProdutoCodigo} não encontrado no Estoque.");

                if (produto.Saldo < item.Quantidade)
                    return await FinalizarComRegistro(idempotencyKey, 400, $"Saldo insuficiente para {produto.Codigo}. Disponível: {produto.Saldo}, solicitado: {item.Quantidade}.");
            }

            foreach (var item in nota.Itens)
            {
                var (sucesso, erro) = await _estoqueApi.BaixarSaldoAsync(item.ProdutoId, item.Quantidade);
                if (!sucesso)
                    return await FinalizarComRegistro(idempotencyKey, 400, $"Falha ao baixar saldo do produto {item.ProdutoCodigo}: {erro}");
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

        return await FinalizarComRegistro(idempotencyKey, 200, nota);
    }

    private async Task<IActionResult> FinalizarComRegistro(string? idempotencyKey, int statusCode, object corpo)
    {
        if (!string.IsNullOrWhiteSpace(idempotencyKey))
        {
            _context.IdempotencyRecords.Add(new IdempotencyRecord
            {
                Chave = idempotencyKey,
                StatusCode = statusCode,
                RespostaJson = JsonSerializer.Serialize(corpo)
            });
            await _context.SaveChangesAsync();
        }

        return StatusCode(statusCode, corpo);
    }
}
