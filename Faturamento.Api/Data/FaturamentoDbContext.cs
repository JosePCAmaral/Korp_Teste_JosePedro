namespace Faturamento.Api.Data
{
    using Faturamento.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class FaturamentoDbContext : DbContext
    {
        public FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : base(options) { }
        public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
        public DbSet<ItemNotaFiscal> Itens => Set<ItemNotaFiscal>();
        public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    }
}
