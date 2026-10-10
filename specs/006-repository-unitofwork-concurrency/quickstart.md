# Developer Quickstart: Using Repositories & Unit of Work in RestoCore

This guide explains how to inject and use the new repository pattern and `IUnitOfWork` in MediatR handlers.

---

## 1. Dependency Injection in Command Handlers

Instead of injecting `IApplicationDbContext`, inject the specific bounded-context repository and `IUnitOfWork`:

```csharp
public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CategoryResponse>
{
    private readonly ICatalogRepository _catalogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICatalogRepository catalogRepository,
        IUnitOfWork unitOfWork)
    {
        _catalogRepository = catalogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            DisplayOrder = request.DisplayOrder
        };

        _catalogRepository.AddCategory(category);
        await _unitOfWork.CommitAsync(cancellationToken);

        return new CategoryResponse(category.Id, category.Name, category.DisplayOrder);
    }
}
```

---

## 2. Using Explicit Transactions

When multiple repositories or external operations must execute atomically:

```csharp
await _unitOfWork.BeginTransactionAsync(cancellationToken);
try
{
    _catalogRepository.AddMenuItem(item);
    _catalogRepository.AddModifierGroup(modifierGroup);
    await _unitOfWork.CommitAsync(cancellationToken);

    await _unitOfWork.CommitTransactionAsync(cancellationToken);
}
catch (Exception)
{
    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
    throw;
}
```

---

## 3. High-Concurrency Item Toggling

For kitchen availability updates, use the resilient repository method which encapsulates optimistic retry handling:

```csharp
public class ToggleItemAvailabilityCommandHandler : IRequestHandler<ToggleItemAvailabilityCommand, ItemAvailabilityResponse>
{
    private readonly IKitchenRepository _kitchenRepository;

    public ToggleItemAvailabilityCommandHandler(IKitchenRepository kitchenRepository)
    {
        _kitchenRepository = kitchenRepository;
    }

    public async Task<ItemAvailabilityResponse> Handle(ToggleItemAvailabilityCommand request, CancellationToken cancellationToken)
    {
        var updated = await _kitchenRepository.UpdateItemAvailabilityWithRetryAsync(
            request.ItemId, 
            request.IsAvailable, 
            maxRetries: 3, 
            cancellationToken);

        if (!updated)
        {
            throw new NotFoundException("MenuItem not found.");
        }

        return new ItemAvailabilityResponse(request.ItemId, request.IsAvailable);
    }
}
```
