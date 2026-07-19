using ApiIngesta.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiIngesta.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(
        DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Infraccion> Infracciones => Set<Infraccion>();

    public DbSet<MensajePendiente> MensajesPendientes
        => Set<MensajePendiente>();
}