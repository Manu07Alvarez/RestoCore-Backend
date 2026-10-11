namespace RestoCore.Application.Features.MenuItems.Commands;

using RestoCore.Application.Common.Mediator;
using RestoCore.Application.Features.MenuItems.DTOs;

public record DeleteMenuItemCommand(Guid Id) : IRequest<MenuItemArchiveResponse>;
