using Menova.Data.Repositories;

namespace Menova.Data.Services
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        public IProductRepository Products { get; }
        public ICategoryRepository Categories { get; }
        public ICartRepository Carts { get; }
        public IOrderRepository Orders { get; }
        public IUserRepository Users { get; }

        public UnitOfWork(ApplicationDbContext context,
                          IProductRepository productRepository,
                          ICategoryRepository categoryRepository,
                          ICartRepository cartRepository,
                          IOrderRepository orderRepository,
                          IUserRepository userRepository)
        {
            _context = context;
            Products = productRepository;
            Categories = categoryRepository;
            Carts = cartRepository;
            Orders = orderRepository;
            Users = userRepository;
        }

        public async Task CompleteAsync()
        {
            await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}
