using MediatR;
using UsersContacts.Interfaces;
using UsersContacts.Mappers;
using UsersContacts.Modules.Contacts.DTOs;

namespace UsersContacts.Modules.Contacts.Commands.CreateContact
{
    public class CreateContactCommand : IRequest<ContactDto>
    {
        public required ContactDto Contact { get; set; }
    }

    public class CreateContactCommandHandler(IUnitOfWork unitOfWork) : IRequestHandler<CreateContactCommand, ContactDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ContactDto> Handle(CreateContactCommand request, CancellationToken cancellationToken)
        {
            var contact = request.Contact.ToEntity();

            await _unitOfWork.ContactRepository.AddAsync(contact);
            await _unitOfWork.SaveChangesAsync();

            return contact.ToDto();
        }
    }
}