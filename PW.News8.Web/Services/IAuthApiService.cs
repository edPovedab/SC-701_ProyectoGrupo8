using PW.News8.Shared.DTOs;
using PW.News8.Web.Models;

namespace PW.News8.Web.Services
{
    public interface IAuthApiService
    {
        Task<LoginResult> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);

        Task LogoutAsync(CancellationToken cancellationToken = default);
    }
}