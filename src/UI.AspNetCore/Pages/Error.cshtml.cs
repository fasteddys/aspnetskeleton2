﻿using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using WebApp.UI.Models;

namespace WebApp.UI.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : CardPageModel<ErrorModel.PageDescriptorClass>
    {
        public string? RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        }

        public sealed class PageDescriptorClass : PageDescriptor
        {
            public override string PageName => "/Error";
            public override Func<HttpContext, IHtmlLocalizer, LocalizedHtmlString> GetDefaultTitle { get; } = (_, t) => t["Error"];
        }
    }
}
