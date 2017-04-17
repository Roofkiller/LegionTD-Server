using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace LegionTDServerReborn.Extension
{
    public static class QueryableExtension
    {
        public static IOrderedQueryable<T> OrderBy<T, T2>(this IQueryable<T> queryable, bool ascending, Expression<Func<T, T2>> selection)
        {
            return (ascending ? queryable.OrderBy(selection) : queryable.OrderByDescending(selection));
        }
    }
}
