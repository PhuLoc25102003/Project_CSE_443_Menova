using Menova.Data.Repositories;

namespace Menova.Data.Services
{
    public interface IUnitOfWork
    {
        IProductRepository Products { get; }
        ICategoryRepository Categories { get; }
        ICartRepository Carts { get; }
        IOrderRepository Orders { get; }
        IUserRepository Users { get; }

        Task CompleteAsync();
    }
}
