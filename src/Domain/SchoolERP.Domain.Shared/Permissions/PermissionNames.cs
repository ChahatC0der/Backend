using System;
using System.Collections.Generic;
using System.Text;

namespace SchoolERP.Domain.Shared.Permissions
{
    public static class PermissionNames
    {
        // ---------- TENANT MODULE ----------
        public const string TenantRead = "tenant.read";
        public const string TenantCreate = "tenant.create";
        public const string TenantUpdate = "tenant.update";
        public const string TenantDelete = "tenant.delete";

        // ---------- USER & RBAC MODULE ----------
        public const string UserRead = "user.read";
        public const string UserCreate = "user.create";
        public const string UserUpdate = "user.update";
        public const string UserDelete = "user.delete";
        public const string UserAssignRole = "user.assign_role";

        // ---------- ACADEMIC STRUCTURE ----------
        public const string ClassRead = "class.read";
        public const string ClassCreate = "class.create";
        public const string ClassUpdate = "class.update";
        public const string ClassDelete = "class.delete";

        public const string SectionRead = "section.read";
        public const string SectionCreate = "section.create";

        public const string SubjectRead = "subject.read";
        public const string SubjectCreate = "subject.create";

        // ---------- STUDENT MODULE ----------
        public const string StudentRead = "student.read";
        public const string StudentCreate = "student.create";
        public const string StudentUpdate = "student.update";
        public const string StudentDelete = "student.delete";
        public const string StudentExport = "student.export";
        public const string StudentAddNote = "student.add_note";

        // ---------- FEES MODULE ----------
        public const string FeeHeadRead = "fee_head.read";
        public const string FeeHeadCreate = "fee_head.create";
        public const string FeeInvoiceRead = "fee_invoice.read";
        public const string FeeInvoiceCreate = "fee_invoice.create";
        public const string FeePaymentCollect = "fee_payment.collect";
        public const string FeeConcessionApprove = "fee_concession.approve";

        // ---------- TRANSPORT MODULE ----------
        public const string TransportRouteRead = "transport_route.read";
        public const string TransportRouteCreate = "transport_route.create";
        public const string TransportVehicleRead = "transport_vehicle.read";
        public const string TransportAssignmentCreate = "transport_assignment.create";

        // ---------- STAFF MODULE ----------
        public const string StaffRead = "staff.read";
        public const string StaffCreate = "staff.create";
        public const string StaffUpdate = "staff.update";
        public const string StaffDelete = "staff.delete";
        public const string StaffAttendanceMark = "staff_attendance.mark";
        public const string StaffLeaveApprove = "staff_leave.approve";
    }
}
