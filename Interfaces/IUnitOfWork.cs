using UsersContacts.Models;

namespace UsersContacts.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Contact> ContactRepository { get; }
        Task<int> SaveChangesAsync();
    }
}
