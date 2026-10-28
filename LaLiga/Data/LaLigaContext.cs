using Microsoft.EntityFrameworkCore;
using LaLiga.Models;

namespace LaLiga.Data;

public class LaLigaContext : DbContext
{
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Match> Matches => Set<Match>();
    public DbSet<Goal> Goals => Set<Goal>();

    public DbSet<PlayerStat> PlayerStats => Set<PlayerStat>();
    public DbSet<TeamStanding> TeamStandings => Set<TeamStanding>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(
            @"Server=localhost;Database=LaLigaDb;Trusted_Connection=True;TrustServerCertificate=True");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Связи для матча (у команды есть домашние и гостевые матчи)
        modelBuilder.Entity<Match>()
            .HasOne(m => m.HomeTeam)
            .WithMany(t => t.HomeMatches)
            .HasForeignKey(m => m.HomeTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Match>()
            .HasOne(m => m.AwayTeam)
            .WithMany(t => t.AwayMatches)
            .HasForeignKey(m => m.AwayTeamId)
            .OnDelete(DeleteBehavior.Restrict);

        // Привязка моделей к представлениям
        modelBuilder.Entity<PlayerStat>().ToView("vw_PlayerStats");
        modelBuilder.Entity<TeamStanding>().ToView("vw_TeamStandings");

        // Начальные данные: команды
        modelBuilder.Entity<Team>().HasData(
            new Team { Id = 1, Name = "Реал Мадрид", City = "Мадрид" },
            new Team { Id = 2, Name = "Барселона", City = "Барселона" },
            new Team { Id = 3, Name = "Атлетико Мадрид", City = "Мадрид" },
            new Team { Id = 4, Name = "Севилья", City = "Севилья" },
            new Team { Id = 5, Name = "Валенсия", City = "Валенсия" }
        );

        // Начальные данные: игроки
        modelBuilder.Entity<Player>().HasData(
            new Player { Id = 1, Name = "Винисиус Жуниор", Position = "Нападающий", TeamId = 1 },
            new Player { Id = 2, Name = "Джуд Беллингем", Position = "Полузащитник", TeamId = 1 },
            new Player { Id = 3, Name = "Роберт Левандовски", Position = "Нападающий", TeamId = 2 },
            new Player { Id = 4, Name = "Ламин Ямаль", Position = "Нападающий", TeamId = 2 },
            new Player { Id = 5, Name = "Антуан Гризманн", Position = "Нападающий", TeamId = 3 },
            new Player { Id = 6, Name = "Юссеф Эн-Несири", Position = "Нападающий", TeamId = 4 },
            new Player { Id = 7, Name = "Уго Дуро", Position = "Нападающий", TeamId = 5 }
        );
    }
}