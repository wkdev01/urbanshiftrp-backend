using Microsoft.EntityFrameworkCore;

namespace UrbanShiftRP;

public class UrbanShiftRPDb : DbContext
{
    public DbSet<Player> Players { get; set; }
    public DbSet<Sessao> Sessoes { get; set; }
    public DbSet<Evento> Eventos { get; set; }
    public DbSet<Transacao> Transacoes { get; set; }
    public DbSet<BanLog> BanLogs { get; set; }

    public UrbanShiftRPDb(DbContextOptions<UrbanShiftRPDb> options) : base(options) { }
}
