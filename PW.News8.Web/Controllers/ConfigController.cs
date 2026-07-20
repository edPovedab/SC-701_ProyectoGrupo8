using Microsoft.AspNetCore.Mvc;
using PW.News8.Web.Services;

namespace PW.News8.Web.Controllers;

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
    public async Task<IActionResult> AssignRole(string email, string role)
    {
        var result = await _configService.AssignRoleAsync(email, role);
        TempData["Message"] = result.Message;
        TempData["Success"] = result.Success.ToString();
        return RedirectToAction(nameof(Index));
    }
}