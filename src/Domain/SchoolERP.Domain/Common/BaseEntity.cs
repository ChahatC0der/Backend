using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Domain.Common
{
    public abstract class BaseEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid TenantId { get; set; }          // Multi-tenancy ka soul
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }    // Soft delete
    }
}
