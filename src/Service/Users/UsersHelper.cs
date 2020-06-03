﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebApp.DataAccess.Entities;

namespace WebApp.Service.Users
{
    internal static class UsersHelper
    {
        private static readonly Expression<Func<User, UserData>> s_toDataExpr = u => new UserData
        {
            UserId = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            IsLockedOut = u.IsLockedOut,
            IsApproved = u.IsApproved,
            CreationDate = u.CreateDate,
            LastPasswordChangedDate = u.LastPasswordChangedDate,
            LastActivityDate = u.LastActivityDate,
            LastLoginDate = u.LastLoginDate,
            LastLockoutDate = u.LastLockoutDate,
        };

        private static readonly Func<User, UserData> s_toData = s_toDataExpr.Compile();

        public static UserData ToData(this User entity) => s_toData(entity);

        public static IQueryable<UserData> ToData(this IQueryable<User> source) => source.Select(s_toDataExpr);

        public static Expression<Func<User, bool>> GetFilterByNameWhere(string name, bool pattern = false)
        {
            if (pattern)
                return u => u.UserName.Contains(name);
            else
                return u => u.UserName == name;
        }

        public static IQueryable<User> FilterByName(this IQueryable<User> source, string name, bool pattern = false)
        {
            return source.Where(GetFilterByNameWhere(name, pattern));
        }

        public static Task<User> GetByNameAsync(this IQueryable<User> source, string name, CancellationToken cancellationToken)
        {
            return source.FilterByName(name).FirstOrDefaultAsync(cancellationToken);
        }

        public static Expression<Func<User, bool>> GetFilterByEmailWhere(string email, bool pattern = false)
        {
            if (pattern)
                return u => u.Email.Contains(email);
            else
                return u => u.Email == email;
        }

        public static IQueryable<User> FilterByEmail(this IQueryable<User> source, string email, bool pattern = false)
        {
            return source.Where(GetFilterByEmailWhere(email, pattern));
        }

        public static Task<User> GetByEmailAsync(this IQueryable<User> source, string email, CancellationToken cancellationToken)
        {
            return source.FilterByEmail(email).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
