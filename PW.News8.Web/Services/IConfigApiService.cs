using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Services;

public interface IConfigApiService
{
    Task<List<UserRoleDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<AuthResponseDto> AssignRoleAsync(string email, string role, CancellationToken cancellationToken = default);
}