using MediatR;
using UsersContacts.Interfaces;

namespace UsersContacts.Modules.Contacts.Commands.DeleteContact
{
    public class DeleteContactCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteContactCommandHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<DeleteContactCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;

        public async Task<bool> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
        {
            var contact = await _unitOfWork.ContactRepository.GetByIdAsync(request.Id);
            ArgumentNullException.ThrowIfNull(contact, nameof(contact));

            await _unitOfWork.ContactRepository.DeleteAsync(contact);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}