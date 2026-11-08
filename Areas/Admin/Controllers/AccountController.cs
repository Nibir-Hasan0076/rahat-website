using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StyleNest.Models.ViewModels;
using StyleNest.Services;

namespace StyleNest.Areas.Admin.Controllers;

[Area("Admin")]
public class AccountController : Controller
{
    private readonly AdminAuthService _auth;

    public AccountController(AdminAuthService auth) => _auth = auth;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(vm);

        var user = await _auth.ValidateAsync(vm.Username.Trim(), vm.Password);
        if (user is null)
        {
            ModelState.AddModelError(string.Empty, "Incorrect username or password.");
            return View(vm);
        }

        await _auth.SignInAsync(HttpContext, user, vm.RememberMe);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Logout()
    {
        await _auth.SignOutAsync(HttpContext);
        return RedirectToAction(nameof(Login));
    }
}
