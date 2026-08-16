using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PW.News8.Shared.DTOs;
using PW.News8.Web.Models;
using PW.News8.Web.Services;

namespace PW.News8.Web.Controllers;

[Authorize(Roles = "Admin")]
public class ConfigController : Controller
{
    private readonly IConfigApiService _configService;

    public ConfigController(IConfigApiService configService)
    {
        _configService = configService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _configService.GetUsersAsync();
        return View(users);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignRole(string email, string role)
    {
        var result = await _configService.AssignRoleAsync(email, role);
        TempData["Message"] = result.Message;
        TempData["Success"] = result.Success.ToString();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Message"] = "Revisa los datos del nuevo usuario: hay campos inválidos.";
            TempData["Success"] = "False";
            return RedirectToAction(nameof(Index));
        }

        var dto = new RegisterDto
        {
            Email = model.Email,
            Password = model.Password,
            Role = model.Role
        };

        var result = await _configService.RegisterAsync(dto);
        TempData["Message"] = result.Message;
        TempData["Success"] = result.Success.ToString();
        return RedirectToAction(nameof(Index));
    }
}