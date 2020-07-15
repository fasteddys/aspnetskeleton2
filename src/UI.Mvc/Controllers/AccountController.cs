﻿using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using WebApp.Service.Infrastructure.Localization;
using WebApp.Service.Infrastructure.Validation;
using WebApp.Service.Settings;
using WebApp.Service.Users;
using WebApp.UI.Filters;
using WebApp.UI.Infrastructure.Localization;
using WebApp.UI.Infrastructure.Security;
using WebApp.UI.Models.Account;

namespace WebApp.UI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly IAccountManager _accountManager;
        private readonly ISettingsProvider _settingsProvider;

        public AccountController(IAccountManager accountManager, ISettingsProvider settingsProvider, IStringLocalizer<AccountController>? stringLocalizer)
        {
            _accountManager = accountManager ?? throw new ArgumentNullException(nameof(accountManager));
            _settingsProvider = settingsProvider ?? throw new ArgumentNullException(nameof(settingsProvider));
            T = stringLocalizer ?? (IStringLocalizer)NullStringLocalizer.Instance;
        }

        public IStringLocalizer T { get; set; }

        private async Task<bool> LoginCoreAsync(LoginModel model, CancellationToken cancellationToken)
        {
            var status = await _accountManager.ValidateUserAsync(model.Credentials, cancellationToken);

            if (status == AuthenticateUserStatus.Successful)
            {
                var claims = new Claim[]
                {
                    new Claim(ClaimTypes.Name, model.UserName)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = model.RememberMe
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return true;
            }
            else
                return false;
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveMenuItem"] = "Login";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null, CancellationToken cancellationToken = default)
        {
            if (ModelState.IsValid)
            {
                if (await LoginCoreAsync(model, cancellationToken))
                    return RedirectToLocal(returnUrl);

                // If we got this far, something failed, redisplay form
                ModelState.AddModelError("", T["Incorrect e-mail address or password."]);
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["ActiveMenuItem"] = "Login";
            return View(model);
        }

        [AllowAnonymous]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();

            return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "" });
        }

        private Task<(CreateUserStatus, PasswordRequirementsData?)> RegisterCoreAsync(RegisterModel model, CancellationToken cancellationToken)
        {
            return _accountManager.CreateUserAsync(model, cancellationToken);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult Register()
        {
            ViewData["ActiveMenuItem"] = "Register";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [AnonymousOnly]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model, CancellationToken cancellationToken)
        {
            if (!_settingsProvider.EnableRegistration())
                return NotFound();

            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await RegisterCoreAsync(model, cancellationToken);

                if (status == CreateUserStatus.Success)
                    return RedirectToAction(nameof(Verify));

                AddModelError(ModelState, status, passwordRequirements);
            }

            ViewData["ActiveMenuItem"] = "Register";
            return View(model);

            void AddModelError(ModelStateDictionary modelState, CreateUserStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case CreateUserStatus.DuplicateUserName:
                    case CreateUserStatus.DuplicateEmail:
                        modelState.AddModelError(nameof(RegisterModel.UserName), T["The e-mail address is already linked to an existing account."]);
                        return;

                    case CreateUserStatus.InvalidPassword:
                        modelState.AddModelError(nameof(RegisterModel.Password), T.LocalizePasswordRequirements(passwordRequirements));
                        return;

                    default:
                        modelState.AddModelError(string.Empty, T["An unexpected error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator."]);
                        return;
                }
            }
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> Verify(string u, string v, CancellationToken cancellationToken)
        {
            bool? model;

            if (u != null && v != null)
                model = await _accountManager.VerifyUserAsync(u, v, cancellationToken);
            else
                model = null;

            ViewData["ActiveMenuItem"] = "Verification";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult ResetPassword(string s)
        {
            var model = new ResetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewData["ActiveMenuItem"] = "Password Reset";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var success = await _accountManager.ResetPasswordAsync(model, cancellationToken);
                return RedirectToAction(null, new { s = Convert.ToInt32(success) });
            }

            ViewData["ActiveMenuItem"] = "Password Reset";
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        [AnonymousOnly]
        public IActionResult SetPassword(string s, string u, string v)
        {
            var model = new SetPasswordModel();
            if (s != null)
                model.Success = Convert.ToBoolean(int.Parse(s));

            ViewData["ActiveMenuItem"] = "New Password";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AllowAnonymous]
        [AnonymousOnly]
        public async Task<IActionResult> SetPassword(SetPasswordModel model, string u, string v, CancellationToken cancellationToken)
        {
            if (ModelState.IsValid)
            {
                var (status, passwordRequirements) = await _accountManager.SetPasswordAsync(u, v, model, cancellationToken);

                if (status != ChangePasswordStatus.InvalidNewPassword)
                    return RedirectToAction(null, new { s = Convert.ToInt32(status == ChangePasswordStatus.Success) });

                AddModelError(ModelState, status, passwordRequirements);
            }

            ViewData["ActiveMenuItem"] = "New Password";
            return View(model);

            void AddModelError(ModelStateDictionary modelState, ChangePasswordStatus status, PasswordRequirementsData? passwordRequirements)
            {
                switch (status)
                {
                    case ChangePasswordStatus.InvalidNewPassword:
                        modelState.AddModelError(nameof(SetPasswordModel.NewPassword), T.LocalizePasswordRequirements(passwordRequirements));
                        return;
                }
            }
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #region Helpers

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);
            else
                return RedirectToAction(nameof(HomeController.Index), "Home", new { area = "Dashboard" });
        }
        
        #endregion
    }
}
