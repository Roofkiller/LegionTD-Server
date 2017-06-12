using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Models
{
    public class LegionTdSqliteContext : DbContext
    {
        public DbSet<Fraction> Fractions { get; set; }
        public DbSet<Unit> Units { get; set; }

        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Duel> Duels { get; set; }
        public DbSet<PlayerMatchData> PlayerMatchDatas { get; set; }
        public DbSet<PlayerUnitRelation> PlayerUnitRelations { get; set; }

        public LegionTdSqliteContext()
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Filename=./legionTdServer.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Duel>().HasKey(d => new { d.MatchId, d.Order });
            modelBuilder.Entity<PlayerMatchData>().HasKey(d => new { d.MatchId, d.PlayerId });
            modelBuilder.Entity<PlayerUnitRelation>().HasKey(d => new { d.MatchId, d.PlayerId, d.UnitName });

            modelBuilder.Entity<Player>().HasMany(p => p.MatchDatas).WithOne(m => m.Player).HasForeignKey(m => m.PlayerId);
            modelBuilder.Entity<PlayerMatchData>().HasMany(m => m.UnitDatas).WithOne(u => u.PlayerMatch).HasForeignKey(u => new {u.MatchId, u.PlayerId});

            modelBuilder.Entity<Match>().Property(m => m.MatchId).ValueGeneratedOnAdd();
        }
    }
}
