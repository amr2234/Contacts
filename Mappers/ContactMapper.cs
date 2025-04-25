using UsersContacts.Models;
using UsersContacts.Modules.Contacts.DTOs;
using UsersContacts.Modules.Contacts.Commands.UpdateContact;

namespace UsersContacts.Mappers
{
    public static class ContactMapper
    {
        public static ContactDto ToDto(this Contact contact)
        {
            return new ContactDto
            {
                Id = contact.Id,
                Name = contact.Name,
                Phone = contact.Phone,
                Address = contact.Address,
                Notes = contact.Notes,
            };
        }

        public static Contact ToEntity(this ContactDto dto)
        {
            return new Contact
            {
                Id = dto.Id,
                Name = dto.Name,
                Phone = dto.Phone,
                Address = dto.Address,
                Notes = dto.Notes ?? string.Empty,
            };
        }
        public static void UpdateEntity(this Contact entity, UpdateContactCommand updateContact)
        {
            entity.Name = updateContact.Contact.Name;
            entity.Phone = updateContact.Contact.Phone;
            entity.Address = updateContact.Contact.Address;
            entity.Notes = updateContact.Contact.Notes ?? string.Empty;
        }
    }
}