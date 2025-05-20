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
        ISizeRepository Sizes { get; }
        IColorRepository Colors { get; }
        IProductVariantRepository ProductVariants { get; }

        Task CompleteAsync();
    }
}
