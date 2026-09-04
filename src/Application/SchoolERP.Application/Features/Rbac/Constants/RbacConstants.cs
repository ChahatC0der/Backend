

namespace SchoolERP.Application.Features.Rbac.Constants
{
    public static class AuditResources
    {
        public const string Role = "role";
        public const string Permission = "permission";
        public const string User = "user";
        public const string RoleAssignment = "role_assignment";
    }

    public static class AuditActions
    {
        public const string Create = "create";
        public const string Update = "update";
        public const string Delete = "delete";
        public const string Assign = "assigne";
        public const string Suspend = "suspend";
    }
}
