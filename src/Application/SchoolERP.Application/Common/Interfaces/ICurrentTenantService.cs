namespace SchoolERP.Application.Common.Interfaces;

public interface ICurrentTenantService
{
    Guid GetTenantId();
    string GetTenantName();
    string GetTenantIdentifier();
}