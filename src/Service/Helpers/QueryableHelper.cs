﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace WebApp.Service.Helpers
{
    public delegate IOrderedQueryable<T> ApplyColumnOrder<T>(IQueryable<T> source, string columnName, bool descending, bool nested);

    public static class QueryableHelper
    {
        private static IOrderedQueryable<T> OrderByCore<T>(this IQueryable<T> source, string keyPath, bool descending, bool nested)
        {
            if (keyPath.Length == 0)
                throw new ArgumentException(null, nameof(keyPath));

            var type = typeof(T);
            var @param = Expression.Parameter(type);

            Expression propertyAccess = @param;
            var propertyNames = keyPath.Split('.');
            for (int i = 0, n = propertyNames.Length; i < n; i++)
                propertyAccess = Expression.Property(propertyAccess, propertyNames[i]);

            var keySelector = Expression.Lambda(propertyAccess, @param);

            var orderQuery = Expression.Call(
                typeof(Queryable),
                !nested ?
                (!descending ? nameof(Queryable.OrderBy) : nameof(Queryable.OrderByDescending)) :
                (!descending ? nameof(Queryable.ThenBy) : nameof(Queryable.ThenByDescending)),
                new[] { type, propertyAccess.Type },
                source.Expression, Expression.Quote(keySelector));

            return (IOrderedQueryable<T>)source.Provider.CreateQuery<T>(orderQuery);
        }

        public static IOrderedQueryable<T> OrderBy<T>(this IQueryable<T> source, string keyPath, bool descending = false)
        {
            return source.OrderByCore(keyPath, descending, nested: false);
        }

        public static IOrderedQueryable<T> ThenBy<T>(this IOrderedQueryable<T> source, string keyPath, bool descending = false)
        {
            return source.OrderByCore(keyPath, descending, nested: true);
        }

        public static (string ColumnName, bool Descending) ParseOrderColumn(string value)
        {
            var c = value[0];
            switch (c)
            {
                case '+':
                case '-':
                    return (value.Substring(1), c == '-');
                default:
                    return (value, false);
            }
        }

        public static string ComposeOrderColumn(string columnName, bool descending)
        {
            return (descending ? '-' : '+') + columnName;
        }

        public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> source, ApplyColumnOrder<T> applyColumnOrder, params string[] orderColumns)
        {
            for (int i = 0, n = orderColumns.Length; i < n; i++)
            {
                var orderColumn = orderColumns[i];

                if (string.IsNullOrEmpty(orderColumn))
                    throw new ArgumentException(null, nameof(orderColumns));

                var (columnName, descending) = ParseOrderColumn(orderColumn);
                source = applyColumnOrder(source, columnName, descending, nested: i > 0);
            }

            return source;
        }

        public static int GetEffectivePageSize(int pageSize, int maxPageSize)
        {
            return maxPageSize > 0 ? Math.Min(pageSize, maxPageSize) : pageSize;
        }

        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> source, int pageIndex, int pageSize, int maxPageSize)
        {
            if (pageIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(pageIndex));

            if (pageSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageSize));

            pageSize = GetEffectivePageSize(pageSize, maxPageSize);

            return source.Skip(pageIndex * pageSize).Take(pageSize);
        }
    }
}
