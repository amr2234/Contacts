using FluentValidation;

namespace UsersContacts.Modules.Contacts.Validators
{
    public static class ValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> ValidName<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage("Name is required")
                .MaximumLength(50)
                .WithMessage("Name cannot exceed 50 characters");
        }

        public static IRuleBuilderOptions<T, string> ValidPhone<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage("Phone is required")
                .Matches(@"^[0-9]{11}$")
                .WithMessage("Phone number must be exactly 11 digits (0-9)");
        }

        public static IRuleBuilderOptions<T, string> ValidAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            return ruleBuilder
                .NotEmpty()
                .WithMessage("Address is required")
                .MaximumLength(200)
                .WithMessage("Address cannot exceed 200 characters");
        }
    }
} 