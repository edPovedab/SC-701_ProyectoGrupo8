using PW.News8.Shared.DTOs;

namespace PW.News8.Web.Models
{
    
    public class LoginResult
    {
        public AuthResponseDto Response { get; set; } = new();
        public string? ApiSessionCookie { get; set; }
    }
}