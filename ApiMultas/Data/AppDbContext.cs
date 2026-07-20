using ApiMultas.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiMultas.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Multa> Multas => Set<Multa>();
}