using FluentValidation;
using PortfolioAPI.Dtos;

namespace PortfolioAPI.Validation;

public class ContactMessageDtoValidator : AbstractValidator<ContactMessageDto>
{
    public ContactMessageDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("Name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .EmailAddress()
            .WithMessage("Invalid email format.")
            .MaximumLength(100)
            .WithMessage("Email cannot exceed 100 characters.");

        RuleFor(x => x.Message)
            .NotEmpty()
            .WithMessage("Message is required.")
            .MaximumLength(1000)
            .WithMessage("Message cannot exceed 1000 characters.");
    }
}