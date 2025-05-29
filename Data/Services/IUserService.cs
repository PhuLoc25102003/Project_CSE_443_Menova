using Menova.Models;

namespace Menova.Data.Services
{
    public interface IUserService
    {
        Task<int> GetTotalUserCountAsync();
    }
}
