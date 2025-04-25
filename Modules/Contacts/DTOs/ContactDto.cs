using System.ComponentModel.DataAnnotations;

namespace UsersContacts.Modules.Contacts.DTOs
{
    public class ContactDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Phone { get; set; }
        public required string Address { get; set; }
        public string? Notes { get; set; }

    }
}