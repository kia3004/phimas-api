using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Data;
using PHIMAS_PREDICTIVE_ANALYTICS.Helpers;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult SignUp()
    {
        return View(new User { Role = "BHW" });
    }

    [HttpPost]
    public async Task<IActionResult> SignUp(User user)
    {
        user.Role = user.Role.Trim().ToUpperInvariant();

        if (!ModelState.IsValid)
        {
            return View(user);
        }

        var email = user.Email.Trim().ToLowerInvariant();
        if (await _context.Users.AnyAsync(existingUser => existingUser.Email.ToLower() == email))
        {
            ModelState.AddModelError(string.Empty, "Email already registered.");
            return View(user);
        }

        user.Email = email;
        user.IsAvailable = user.Role == "BHW";
        user.Password = PasswordHelper.HashPassword(user.Password);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        await SignInUserAsync(user);
        return RedirectBasedOnRole(user.Role);
    }

    [HttpGet]
    public IActionResult SignIn()
    {
        return View(new User { Role = "BHW" });
    }

    [HttpPost]
    public async Task<IActionResult> SignIn(string email, string role, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "All fields are required.");
            return View(new User { Role = role });
        }

        var normalizedRole = role.Trim().ToUpperInvariant();
        var normalizedEmail = email.Trim().ToLowerInvariant();

        var user = await _context.Users.FirstOrDefaultAsync(existingUser =>
            existingUser.Email.ToLower() == normalizedEmail && existingUser.Role == normalizedRole);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or role.");
            return View(new User { Role = normalizedRole, Email = normalizedEmail });
        }

        if (!PasswordHelper.VerifyPassword(password, user.Password))
        {
            ModelState.AddModelError(string.Empty, "Incorrect password.");
            return View(new User { Role = normalizedRole, Email = normalizedEmail });
        }

        await SignInUserAsync(user);
        return RedirectBasedOnRole(user.Role);
    }

    [HttpGet]
    [ActionName("SignOut")]
    public async Task<IActionResult> SignOutUser()
    {
        HttpContext.Session.Clear();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    private async Task SignInUserAsync(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserID.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        HttpContext.Session.SetInt32("UserID", user.UserID);
        HttpContext.Session.SetString("UserRole", user.Role);
        HttpContext.Session.SetString("UserName", user.FullName);
    }

    private IActionResult RedirectBasedOnRole(string role)
    {
        return role switch
        {
            "ADMIN" => RedirectToAction("Dashboard", "Admin"),
            "CHO" => RedirectToAction("Dashboard", "CHO"),
            "BHW" => RedirectToAction("Dashboard", "BHW"),
            _ => RedirectToAction(nameof(SignIn))
        };
    }
}
