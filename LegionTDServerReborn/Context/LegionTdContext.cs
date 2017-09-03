using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace LegionTDServerReborn.Models
{
    public class LegionTdContext : DbContext
    {
        public DbSet<Fraction> Fractions { get; set; }
        public DbSet<Unit> Units { get; set; }

        public DbSet<Player> Players { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Duel> Duels { get; set; }
        public DbSet<PlayerMatchData> PlayerMatchDatas { get; set; }
        public DbSet<PlayerUnitRelation> PlayerUnitRelations { get; set; }
        public DbSet<Ranking> Rankings { get; set; }
        public DbSet<FractionStatistic> FractionStatistic {get; set;}

        public LegionTdContext(DbContextOptions<LegionTdContext> options)
            :base (options) {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Duel>().HasKey(d => new { d.MatchId, d.Order });
            modelBuilder.Entity<PlayerMatchData>().HasKey(d => new { d.MatchId, d.PlayerId });
            modelBuilder.Entity<PlayerUnitRelation>().HasKey(d => new { d.MatchId, d.PlayerId, d.UnitName });
            // modelBuilder.Entity<FractionData>().HasKey(d => new {d.MatchId, d.PlayerId, d.FractionName});
            modelBuilder.Entity<FractionStatistic>().HasKey(f => new {f.FractionName, f.TimeStamp});
            modelBuilder.Entity<Ranking>().HasKey(r => new { r.Type, r.Ascending, r.PlayerId});

            modelBuilder.Entity<Player>().HasMany(p => p.Rankings).WithOne(r => r.Player).HasForeignKey(r => r.PlayerId);
            modelBuilder.Entity<Player>().HasMany(p => p.MatchDatas).WithOne(m => m.Player).HasForeignKey(m => m.PlayerId);
            modelBuilder.Entity<PlayerMatchData>().HasMany(m => m.UnitDatas).WithOne(u => u.PlayerMatch).HasForeignKey(u => new {u.MatchId, u.PlayerId});
            // modelBuilder.Entity<PlayerMatchData>().HasMany(p => p.FractionDatas).WithOne(d => d.PlayerMatch).HasForeignKey(d => new {d.MatchId, d.PlayerId});
            modelBuilder.Entity<Fraction>().HasMany(f => f.Statistics).WithOne(f => f.Fraction).HasForeignKey(f => f.FractionName);

            modelBuilder.Entity<Unit>().HasOne(u => u.Parent).WithMany(p => p.Children).HasForeignKey(u => u.ParentName);

            modelBuilder.Entity<Match>().Property(m => m.MatchId).ValueGeneratedOnAdd();
            
            modelBuilder.Entity<Ranking>().HasIndex(r => r.Position);

        }
    }
}
