using FluentValidation;
using UsersContacts.Modules.Contacts.DTOs;

namespace UsersContacts.Modules.Contacts.Validators
{
    public class ContactDtoValidator : AbstractValidator<ContactDto>
    {
        public ContactDtoValidator()
        {
            RuleFor(x => x.Name).ValidName();
            RuleFor(x => x.Phone).ValidPhone();
            RuleFor(x => x.Address).ValidAddress();
        }
    }
}