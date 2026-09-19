namespace RestoCore.Application.Features.MenuLayout.Validators;

using FluentValidation;
using RestoCore.Application.Features.MenuLayout.Commands;
using RestoCore.Application.Features.MenuLayout.DTOs;

public class CanvasElementValidator : AbstractValidator<CanvasElementDto>
{
    public CanvasElementValidator()
    {
        RuleFor(x => x.DishId)
            .NotEmpty().WithMessage("Dish ID is required.");

        RuleFor(x => x.X)
            .GreaterThanOrEqualTo(0).WithMessage("X coordinate cannot be negative.");

        RuleFor(x => x.Y)
            .GreaterThanOrEqualTo(0).WithMessage("Y coordinate cannot be negative.");

        RuleFor(x => x.ZIndex)
            .GreaterThanOrEqualTo(0).WithMessage("ZIndex cannot be negative.");

        RuleFor(x => x.Width)
            .GreaterThan(0).WithMessage("Width must be greater than 0.");

        RuleFor(x => x.Height)
            .GreaterThan(0).WithMessage("Height must be greater than 0.");
    }
}

public class UpdateMenuLayoutValidator : AbstractValidator<UpdateMenuLayoutCommand>
{
    public UpdateMenuLayoutValidator()
    {
        RuleFor(x => x.Layout)
            .NotNull().WithMessage("Layout configuration is required.");

        When(x => x.Layout != null, () =>
        {
            RuleFor(x => x.Layout.BackgroundUrl)
                .MaximumLength(500).WithMessage("Background URL cannot exceed 500 characters.");

            RuleFor(x => x.Layout.BackgroundColor)
                .MaximumLength(50).WithMessage("Background color cannot exceed 50 characters.");

            RuleForEach(x => x.Layout.Elements)
                .SetValidator(new CanvasElementValidator());
        });
    }
}