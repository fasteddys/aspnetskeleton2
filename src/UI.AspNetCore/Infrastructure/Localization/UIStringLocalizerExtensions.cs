﻿using System;
using System.Text;
using Karambolo.Common.Localization;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Validation;

namespace WebApp.UI.Infrastructure.Localization
{
    public static class UIStringLocalizerExtensions
    {
        public static string LocalizePasswordRequirements(this IStringLocalizer stringLocalizer, PasswordRequirementsData? passwordRequirements)
        {
            var t = stringLocalizer;

            var errorMessage = t["The password does not meet the complexity requirements."];

            if (passwordRequirements == null)
                return errorMessage;

            var sb = new StringBuilder(errorMessage);

            sb.Append(' ');
            sb.Append(t["Passwords must be at least {0} character long.", Plural.From("Passwords must be at least {0} characters long.", Math.Max(1, passwordRequirements!.RequiredLength))]);

            if (passwordRequirements.RequireNonAlphanumeric || passwordRequirements.RequireDigit || passwordRequirements.RequireLowercase || passwordRequirements.RequireUppercase)
            {
                sb.Append(' ');
                sb.Append(t["Must contain at least one character from the following category(s):"]).Append(' ');

                if (passwordRequirements.RequireNonAlphanumeric)
                    sb.Append(t["non-alphabetic characters (!, $, #, etc.)"]).Append(", ");

                if (passwordRequirements.RequireDigit)
                    sb.Append(t["digits (0-9)"]).Append(", ");

                if (passwordRequirements.RequireLowercase)
                    sb.Append(t["lowercase characters (a-z)"]).Append(", ");

                if (passwordRequirements.RequireUppercase)
                    sb.Append(t["uppercase characters (A-Z)"]).Append(", ");

                sb.Remove(sb.Length - 2, 2).Append('.');
            }

            if (passwordRequirements.RequiredUniqueChars > 1)
            {
                sb.Append(' ');
                sb.Append(t["Must contain at least {0} different character.", Plural.From("Must contain at least {0} different characters.", passwordRequirements.RequiredUniqueChars)]);
            }

            return sb.ToString();
        }
    }
}
