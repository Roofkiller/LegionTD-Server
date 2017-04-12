using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

        public LegionTdContext()
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
            /*
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.Won)
                .HasComputedColumnSql("[Team] = (SELECT Winner " +
                                      "FROM Match AS m " +
                                      "WHERE m.MatchId = [MatchId])");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.WonDuels)
                .HasComputedColumnSql("SELECT COUNT(*) " +
                                      "FROM Duel AS d " +
                                      "WHERE d.MatchId = [MatchId] " +
                                      "AND d.Winner = [Team]");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.LostDuels)
                .HasComputedColumnSql("SELECT COUNT(*) " +
                                      "FROM Duel AS d " +
                                      "WHERE d.MatchId = [MatchId] " +
                                      "AND d.Winner != [Team]");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.Experience)
                .HasComputedColumnSql("SELECT COALESCE(SUM(pur.Killed * " +
                                      "(SELECT Experience " +
                                      "FROM Units AS u " +
                                      "WHERE u.Name = pur.UnitName)),0) " +
                                      "FROM PlayerUnitRelations AS pur " +
                                      "WHERE pur.MatchId = [MatchId] " +
                                      "AND pur.PlayerId = [PlayerId]");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.Kills)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Killed),0) " +
                                      "FROM PlayerUnitRelations AS pur " +
                                      "WHERE pur.MatchId = [MatchId] " +
                                      "AND pur.PlayerId = [PlayerId] ");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.Leaks)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Leaked),0) " +
                                      "FROM PlayerUnitRelations AS pur " +
                                      "WHERE pur.MatchId = [MatchId] " +
                                      "AND pur.PlayerId = [PlayerId] ");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.Sends)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Send),0) " +
                                      "FROM PlayerUnitRelations AS pur " +
                                      "WHERE pur.MatchId = [MatchId] " +
                                      "AND pur.PlayerId = [PlayerId] ");
            modelBuilder.Entity<PlayerMatchData>()
                .Property(p => p.Builds)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Build),0) " +
                                      "FROM PlayerUnitRelations AS pur " +
                                      "WHERE pur.MatchId = [MatchId] " +
                                      "AND pur.PlayerId = [PlayerId] ");



            modelBuilder.Entity<Player>()
                .Property(p => p.WonGames)
                .HasComputedColumnSql("SELECT COUNT(*) " +
                                      "FROM PlayerMatchDatas AS pmd " +
                                      "WHERE pmd.PlayerId = [PlayerId] " +
                                      "AND pmd.Won = 1");

            modelBuilder.Entity<Player>()
                .Property(p => p.LostGames)
                .HasComputedColumnSql("SELECT COUNT(*) " +
                                      "FROM PlayerMatchDatas AS pmd " +
                                      "WHERE pmd.PlayerId = [PlayerId] " +
                                      "AND pmd.Won = 0");

            modelBuilder.Entity<Player>()
                .Property(p => p.PlayedGames)
                .HasComputedColumnSql("SELECT COUNT(*) " +
                                      "FROM PlayerMatchDatas AS pmd " +
                                      "WHERE pmd.PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.WinRate)
                .HasComputedColumnSql("[WonGames]/[PlayedGames]");

            modelBuilder.Entity<Player>()
                .Property(p => p.WonDuels)
                .HasComputedColumnSql("SELECT COALESCE(SUM(WonDuels),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.LostDuels)
                .HasComputedColumnSql("SELECT COALESCE(SUM(LostDuels),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.PlayedDuels)
                .HasComputedColumnSql("SELECT COALESCE(SUM(LostDuels) + SUM(WonDuels),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.DuelWinRate)
                .HasComputedColumnSql("[WonDuels]/[PlayedDuels]");

            modelBuilder.Entity<Player>()
                .Property(p => p.Experience)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Experience),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.Kills)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Kills),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.Leaks)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Leaks),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.Builds)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Builds),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.Sends)
                .HasComputedColumnSql("SELECT COALESCE(SUM(Sends),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");

            modelBuilder.Entity<Player>()
                .Property(p => p.EarnedTangos)
                .HasComputedColumnSql("SELECT COALESCE(SUM(EarnedTangos),0) " +
                                      "FROM PlayerMatchDatas " +
                                      "WHERE PlayerId = [PlayerId]");
            */
        }
    }
}
