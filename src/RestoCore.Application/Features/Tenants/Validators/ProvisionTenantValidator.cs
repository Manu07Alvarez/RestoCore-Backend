namespace RestoCore.Application.Features.Tenants.Validators;

using FluentValidation;
using RestoCore.Application.Features.Tenants.Commands;

public class ProvisionTenantValidator : AbstractValidator<ProvisionTenantCommand>
{
    public ProvisionTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required.")
            .MaximumLength(100).WithMessage("Tenant name cannot exceed 100 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Tenant slug is required.")
            .MaximumLength(50).WithMessage("Tenant slug cannot exceed 50 characters.")
            .Matches("^[a-z0-9-]+$").WithMessage("Tenant slug may only contain lowercase alphanumeric characters and hyphens.");

        RuleFor(x => x.AdminEmail)
            .NotEmpty().WithMessage("Admin email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.CustomDomain)
            .MaximumLength(150).WithMessage("Custom domain cannot exceed 150 characters.");
    }
}
