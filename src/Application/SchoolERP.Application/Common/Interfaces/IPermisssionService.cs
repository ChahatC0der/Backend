namespace SchoolERP.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<bool> HasPermissionAsync(long userId, string permissionKey, CancellationToken cancellationToken);
}