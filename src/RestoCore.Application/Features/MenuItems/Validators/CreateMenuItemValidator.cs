namespace RestoCore.Application.Features.MenuItems.Validators;

using FluentValidation;
using RestoCore.Application.Features.MenuItems.Commands;

public class CreateMenuItemValidator : AbstractValidator<CreateMenuItemCommand>
{
    public CreateMenuItemValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty().WithMessage("Category ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu item name is required.")
            .MaximumLength(100).WithMessage("Menu item name cannot exceed 100 characters.");

        RuleFor(x => x.BasePrice)
            .GreaterThanOrEqualTo(0.00m).WithMessage("Base price cannot be negative.");

        RuleFor(x => x.ImageUrl)
            .MaximumLength(300).WithMessage("Image URL cannot exceed 300 characters.");

        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0).WithMessage("Display order must be greater than or equal to 0.");
    }
}
