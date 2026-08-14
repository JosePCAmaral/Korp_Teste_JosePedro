namespace Estoque.Api.Data
{
    using Estoque.Api.Models;
    using Microsoft.EntityFrameworkCore;

    public class EstoqueDbContext : DbContext
    {
        public EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : base(options) { }
        public DbSet<Produto> Produtos => Set<Produto>();
    }
}
