namespace SchoolERP.Domain.Common;

public abstract class IdEntity
{
    public long Id { get; set; }
}

public abstract class GuidEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}

public interface IAuditableTimestamps
{
    DateTime CreatedAt { get; set; }
    DateTime? UpdatedAt { get; set; }
}

// 🔥 Normal entities (Student, Fee, etc.) — Id long hai


public abstract class BaseEntity : IdEntity, IAuditableTimestamps
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

// 🔥🔥 SIRF Tenant aur Branch ke liye — Id khud Guid hai
public abstract class GuidAuditableEntity : GuidEntity,IAuditableTimestamps
{
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
}

public abstract class BaseAuditLog
{
    public long Id { get; set; }
    public Guid? TenantId { get; set; }        // platform-level ke liye nullable
    public long PerformedBy { get; set; }      // user id
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

// 🔥 Marker interfaces — TenantId/BranchId Guid hi rahenge
public interface IMustHaveTenant
{
    Guid TenantId { get; set; }
}

public interface IMustHaveBranch : IMustHaveTenant
{
    Guid BranchId { get; set; }
}

// 🔥 Normal entities jo Tenant se link honi hain
public abstract class BaseTenantEntity : BaseEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
}

public abstract class TenantAuditableEntity : BaseAuditableEntity, IMustHaveTenant
{
    public Guid TenantId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public abstract class BranchEntity : BaseEntity, IMustHaveBranch
{
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
}

public abstract class BranchAuditableEntity : BaseAuditableEntity, IMustHaveBranch
{
    public Guid TenantId { get; set; }
    public Guid BranchId { get; set; }
}