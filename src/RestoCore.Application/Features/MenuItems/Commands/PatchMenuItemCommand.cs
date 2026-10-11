namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Mediator;

public record PatchMenuItemCommand(
    Guid Id,
    Guid? CategoryId,
    string? Name,
    string? Description,
    decimal? BasePrice,
    bool? IsAvailable,
    string? ImageUrl,
    int? DisplayOrder,
    string[]? Allergens,
    string[]? DietaryLabels
) : IRequest<MenuItemResponse>;
