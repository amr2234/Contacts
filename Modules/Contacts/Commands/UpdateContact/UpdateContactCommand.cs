using MediatR;
using UsersContacts.Interfaces;
using UsersContacts.Mappers;
using UsersContacts.Modules.Contacts.DTOs;

namespace UsersContacts.Modules.Contacts.Commands.UpdateContact
{
    public class UpdateContactCommand : IRequest<ContactDto>
    {
        public int Id { get; set; }
        public required ContactDto Contact { get; set; }
    }

    public class UpdateContactCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<UpdateContactCommand, ContactDto>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<ContactDto> Handle(UpdateContactCommand request, CancellationToken cancellationToken)
        {
            var existingContact = await _unitOfWork.ContactRepository.GetByIdAsync(request.Id);
            ArgumentNullException.ThrowIfNull(existingContact, nameof(existingContact));

            existingContact.UpdateEntity(request);

            await _unitOfWork.SaveChangesAsync();
            return existingContact.ToDto();
        }

    }
}