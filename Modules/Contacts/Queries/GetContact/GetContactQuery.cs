using MediatR;
using UsersContacts.Interfaces;
using UsersContacts.Mappers;
using UsersContacts.Modules.Contacts.DTOs;

namespace UsersContacts.Modules.Contacts.Queries.GetContact
{
    public class GetContactQuery : IRequest<ContactDto?>
    {
        public int Id { get; set; }
    }

    public class GetContactQueryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<GetContactQuery, ContactDto?>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ContactDto?> Handle(GetContactQuery request, CancellationToken cancellationToken)
        {
            var contact = await _unitOfWork.ContactRepository.GetByIdAsync(request.Id);
            return contact?.ToDto();
        }
    }
}