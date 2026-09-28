using System.Security.Claims;
using MarkdownWiki.Core.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MarkdownWiki.Pages.Admin;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly WikiOptions _options;

    public LoginModel(WikiOptions options)
    {
        _options = options;
    }

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; private set; }

    public IActionResult OnGet()
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        if (User.Identity?.IsAuthenticated == true)
        {
            return LocalRedirect(SafeReturnUrl(ReturnUrl));
        }

        ViewData["Title"] = "管理员登录";
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!_options.EnableEdit)
        {
            return RedirectToPage("/Index");
        }

        var expected = _options.AdminPassword ?? "123456";
        if (!string.Equals(Password, expected, StringComparison.Ordinal))
        {
            ErrorMessage = "密码错误。";
            ViewData["Title"] = "管理员登录";
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "admin"),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            });

        return LocalRedirect(SafeReturnUrl(ReturnUrl));
    }

    private static string SafeReturnUrl(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            returnUrl.StartsWith('/') &&
            !returnUrl.StartsWith("//", StringComparison.Ordinal))
        {
            return returnUrl;
        }

        return "/Admin";
    }
}
