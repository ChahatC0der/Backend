namespace SchoolERP.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid GetTenantId();
    Guid GetBranchId();
    string GetTenantName();
    string GetTenantIdentifier();
}