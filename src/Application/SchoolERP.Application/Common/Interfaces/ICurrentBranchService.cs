namespace SchoolERP.Application.Common.Interfaces;

public interface ICurrentBranchService
{
    Guid? GetBranchId();
    bool IsBranchResolved { get; }
}