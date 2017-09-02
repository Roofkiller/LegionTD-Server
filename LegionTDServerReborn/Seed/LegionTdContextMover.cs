using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LegionTDServerReborn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LegionTDServerReborn.Seed
{
    public static class LegionTdContextMover
    {

        // public static void SetTraining()
        // {
        //         var matches = db.Matches.Include(m => m.PlayerDatas).Where(m => m.Date > DateTime.Parse("2017-05-10 12:25:04.366187")).ToList();
        //         int i = 0;
        //         matches.ForEach(m =>
        //         {
        //             m.IsTraining = m.PlayerDatas.All(p => p.Team == m.Winner) ||
        //                            m.PlayerDatas.All(p => p.Team != m.Winner);
        //             if (m.IsTraining)
        //                 m.PlayerDatas.ForEach(p =>
        //                 {
        //                     p.RatingChange = 0;
        //                 });
        //                 Console.WriteLine(i + " geschafft.");
        //             i++;
        //         });
        //         db.SaveChanges();
        // }

        // public static async void Seed()
        // {
        //     using (var oldC = new LegionTdSqliteContext())
        //     {
        //         using (var newC = new LegionTdContext()) {
        //             newC.Fractions.AddRange(oldC.Fractions.ToList());
        //             await newC.SaveChangesAsync();
        //             Console.WriteLine("Added fractions");
        //             newC.Units.AddRange(oldC.Units.ToList());
        //             await newC.SaveChangesAsync();
        //             Console.WriteLine("Added Units");
        //             newC.Players.AddRange(oldC.Players);
        //             await newC.SaveChangesAsync();
        //             Console.WriteLine("Added Players");
        //             newC.Matches.AddRange(oldC.Matches);
        //             await newC.SaveChangesAsync();
        //             Console.WriteLine("Added Matches");
        //             newC.Duels.AddRange(oldC.Duels);
        //             await newC.SaveChangesAsync();
        //             Console.WriteLine("Added Duels");
        //             newC.PlayerMatchDatas.AddRange(oldC.PlayerMatchDatas);
        //             await newC.SaveChangesAsync();
        //             Console.WriteLine("Added PlayerMatchDatas");
        //         }
        //         int stepSize = 20000;
        //         IQueryable<PlayerUnitRelation> query = oldC.PlayerUnitRelations;
        //         int index = 0;
        //         int count = 5560491;
        //         oldC.ChangeTracker.AutoDetectChangesEnabled = false;
        //         while (index <= count)
        //         {
        //             using (var newC = new LegionTdContext())
        //             {
        //                 newC.ChangeTracker.AutoDetectChangesEnabled = false;
        //                 var relations = (query = query.Skip(index)).Take(stepSize);
        //                 newC.PlayerUnitRelations.AddRange(relations);
        //                 newC.SaveChanges();
        //                 index += stepSize;
        //                 Console.WriteLine("Added " + index + " relations.");
        //             }
        //             GC.Collect();
        //         }
        //         Console.WriteLine("Added PlayerUnitRealations");
        //     }
        // }
    }
}
