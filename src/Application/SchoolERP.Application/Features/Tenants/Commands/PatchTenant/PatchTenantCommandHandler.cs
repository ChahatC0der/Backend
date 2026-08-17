using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Tenants.DTOs;
using SchoolERP.Domain.Shared.Results;
using SchoolERP.Domain.Tenants.Entities;

namespace SchoolERP.Application.Features.Tenants.Commands.PatchTenant;

public class PatchTenantCommandHandler : IRequestHandler<PatchTenantCommand, Result<TenantResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public PatchTenantCommandHandler(IApplicationDbContext dbContext)
        => _dbContext = dbContext;

    public async Task<Result<TenantResponse>> Handle(PatchTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await _dbContext.Set<Tenant>()
            .FirstOrDefaultAsync(t => t.Id == request.Id && !t.IsDeleted, cancellationToken);

        if (tenant == null)
            return Error.NotFound("Tenant", request.Id.ToString());

        // 🔥 Sirf jo fields request mein aaye hain, unhe update karo
        if (!string.IsNullOrEmpty(request.Request.Code))
            tenant.Code = request.Request.Code;

        if (!string.IsNullOrEmpty(request.Request.Name))
            tenant.Name = request.Request.Name;

        if (!string.IsNullOrEmpty(request.Request.Subdomain))
            tenant.Subdomain = request.Request.Subdomain.Trim().ToLower().Replace(" ", "-");

        if (!string.IsNullOrEmpty(request.Request.ContactEmail))
            tenant.ContactEmail = request.Request.ContactEmail;

        if (!string.IsNullOrEmpty(request.Request.ContactPhone))
            tenant.ContactPhone = request.Request.ContactPhone;

        if (!string.IsNullOrEmpty(request.Request.Address))
            tenant.Address = request.Request.Address;

        if (!string.IsNullOrEmpty(request.Request.Plan))
            tenant.Plan = request.Request.Plan;

        if (!string.IsNullOrEmpty(request.Request.Status))
            tenant.Status = request.Request.Status;

        tenant.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(tenant.Adapt<TenantResponse>(), "Tenant updated successfully.");
    }
}