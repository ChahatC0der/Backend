using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Domain.Common
{
    public abstract class BaseAuditableEntity
    {
        public long? PerformedBy { get; set; }
        
    }
}
