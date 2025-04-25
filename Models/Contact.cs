using System.ComponentModel.DataAnnotations;

namespace UsersContacts.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        [RegularExpression(@"^[0-9]{11}$", ErrorMessage = "Phone number must be exactly 11 digits (0-9)")]
        public required string Phone { get; set; }

        [Required(ErrorMessage = "Address is required")]
        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters")]
        public required string Address { get; set; }

        [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
        public string? Notes { get; set; }

    }
} 