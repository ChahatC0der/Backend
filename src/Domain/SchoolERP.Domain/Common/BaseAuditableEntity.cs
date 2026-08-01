using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Domain.Common
{
    public abstract class BaseAuditableEntity : BaseEntity
    {
        public Guid? CreatedBy { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
