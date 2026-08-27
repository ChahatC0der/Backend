namespace SchoolERP.Application.Common.Interfaces;

public interface ICurrentUserService
{
    long? GetUserId();
    bool IsAuthenticated { get; }
}