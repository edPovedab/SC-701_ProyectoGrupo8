using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PW.News8.Shared.DTOs;

namespace PW.News8.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AuthController(UserManager<IdentityUser> userManager,
                          SignInManager<IdentityUser> signInManager,
                          RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Credenciales inválidas." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return Unauthorized(new AuthResponseDto { Success = false, Message = "Credenciales inválidas." });

        await _signInManager.SignInAsync(user, isPersistent: false);

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new AuthResponseDto
        {
            Success = true,
            Message = "Login exitoso.",
            Email = user.Email,
            Role = roles.FirstOrDefault()
        });
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new AuthResponseDto { Success = true, Message = "Sesión cerrada." });
    }

    [HttpPost("register")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return BadRequest(new AuthResponseDto { Success = false, Message = $"El rol '{dto.Role}' no existe." });

        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return BadRequest(new AuthResponseDto { Success = false, Message = errors });
        }

        await _userManager.AddToRoleAsync(user, dto.Role);
        return Ok(new AuthResponseDto { Success = true, Message = $"Usuario '{dto.Email}' creado con rol '{dto.Role}'." });
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetUsers()
    {
        var users = _userManager.Users.ToList();
        var result = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new
            {
                user.Id,
                user.Email,
                Role = roles.FirstOrDefault() ?? "Sin rol"
            });
        }

        return Ok(result);
    }

    [HttpPut("assign-role")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole([FromBody] RegisterDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return NotFound(new AuthResponseDto { Success = false, Message = "Usuario no encontrado." });

        if (!await _roleManager.RoleExistsAsync(dto.Role))
            return BadRequest(new AuthResponseDto { Success = false, Message = $"El rol '{dto.Role}' no existe." });

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        await _userManager.AddToRoleAsync(user, dto.Role);

        return Ok(new AuthResponseDto { Success = true, Message = $"Rol '{dto.Role}' asignado a '{dto.Email}'." });
    }
}