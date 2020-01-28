using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace LegionTDServerReborn.Extensions
{
    public static class DbContextExtension
    {
        public static async Task<List<T1>> GetOrCreateAsync<T1, T2>(this DbContext db, IEnumerable<T2> ids, Func<T1, T2> identifierFunc, Func<T2, T1> initFunc, string property = null) where T1 : class
        {
            // Gather basic info
            var dbSet = db.Set<T1>();
            var entityType = db.Model.GetEntityTypes().First(t => t.ClrType == typeof(T1));
            var tableName = entityType.GetTableName();
            var discrProp = entityType.GetDiscriminatorProperty();
            var discrName = entityType.GetDiscriminatorValue();
            if (string.IsNullOrWhiteSpace(property))
            {
                property = entityType.FindPrimaryKey().Properties[0].Name;
            }

            var idString = string.Join(", ", ids.Select(id => $"'{id}'"));
            var sql = $"SELECT * FROM {tableName} WHERE {property} IN ({idString})";
            if (discrProp != null)
            {
                sql += $" AND {discrProp.Name} = '{discrName}'";
            }
            var existingData = await dbSet.FromSqlRaw(sql).ToDictionaryAsync(o => identifierFunc(o), o => o);
            var result = new List<T1>();
            foreach (var id in ids)
            {
                if (!existingData.ContainsKey(id))
                {
                    var newData = initFunc(id);
                    existingData[id] = newData;
                    dbSet.Add(newData);
                }
                result.Add(existingData[id]);
            }
            return result;
        }
    }
}
