using UsersContacts.AppDbContext;
using UsersContacts.Interfaces;
using UsersContacts.Models;

namespace UsersContacts.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly IRepository<Contact> _contactRepository;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            _contactRepository = new Repository<Contact>(context);
        }

        public IRepository<Contact> ContactRepository => _contactRepository;

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}