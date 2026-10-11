namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Mediator;

public record RestoreMenuItemCommand(Guid Id) : IRequest<MenuItemResponse>;
