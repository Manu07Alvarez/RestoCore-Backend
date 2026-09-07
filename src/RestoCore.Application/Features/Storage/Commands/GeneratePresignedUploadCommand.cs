namespace RestoCore.Application.Features.Storage.Commands;

using FluentValidation;
using MediatR;
using RestoCore.Application.Common.Interfaces;

public record GeneratePresignedUploadCommand(
    string FileName,
    string ContentType,
    string Category
) : IRequest<PresignedUploadResponse>;

public class GeneratePresignedUploadValidator : AbstractValidator<GeneratePresignedUploadCommand>
{
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private static readonly string[] AllowedCategories = { "dishes", "branding" };

    public GeneratePresignedUploadValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(150).WithMessage("File name cannot exceed 150 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required.")
            .Must(ct => AllowedContentTypes.Contains(ct.ToLowerInvariant()))
            .WithMessage("Content type must be image/jpeg, image/png, or image/webp.");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required.")
            .Must(cat => AllowedCategories.Contains(cat.ToLowerInvariant()))
            .WithMessage("Category must be either 'dishes' or 'branding'.");
    }
}

public class GeneratePresignedUploadCommandHandler : IRequestHandler<GeneratePresignedUploadCommand, PresignedUploadResponse>
{
    private readonly IStorageService _storageService;
    private readonly ITenantContext _tenantContext;

    public GeneratePresignedUploadCommandHandler(IStorageService storageService, ITenantContext tenantContext)
    {
        _storageService = storageService;
        _tenantContext = tenantContext;
    }

    public async Task<PresignedUploadResponse> Handle(GeneratePresignedUploadCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required.");

        return await _storageService.GeneratePresignedUploadUrlAsync(
            tenantId,
            request.FileName,
            request.ContentType.ToLowerInvariant(),
            request.Category.ToLowerInvariant(),
            cancellationToken);
    }
}
