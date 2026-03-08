using System.Threading.Tasks;
using OCC.Shared.Models;

namespace OCC.WpfClient.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool Success, string ErrorMessage)> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(User user);
        Task LogoutAsync();
        User? CurrentUser { get; }
        bool IsAuthenticated { get; }
    }
}
