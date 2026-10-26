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
        // Fee Heads
        public const string FeeHeadRead = "fee_head.read";
        public const string FeeHeadCreate = "fee_head.create";
        public const string FeeHeadUpdate = "fee_head.update";
        public const string FeeHeadDelete = "fee_head.delete";
        public const string FeeHeadRestore = "fee_head.restore";
        public const string FeeHeadBulkDelete = "fee_head.bulk_delete";
        public const string FeeHeadSeedDefaults = "fee_head.seed_defaults";
        public const string FeeHeadExport = "fee_head.export";

        // Fee Structures
        public const string FeeStructureRead = "fee_structure.read";
        public const string FeeStructureCreate = "fee_structure.create";
        public const string FeeStructureUpdate = "fee_structure.update";
        public const string FeeStructureDelete = "fee_structure.delete";
        public const string FeeStructureClone = "fee_structure.clone";
        public const string FeeStructureAssign = "fee_structure.assign";
        public const string FeeStructureExport = "fee_structure.export";

        // Invoices
        public const string FeeInvoiceRead = "fee_invoice.read";
        public const string FeeInvoiceCreate = "fee_invoice.create";
        public const string FeeInvoiceUpdate = "fee_invoice.update";
        public const string FeeInvoiceCancel = "fee_invoice.cancel";
        public const string FeeInvoiceRegenerate = "fee_invoice.regenerate";
        public const string FeeInvoiceAdjust = "fee_invoice.adjust";
        public const string FeeInvoiceWaiveLateFee = "fee_invoice.waive_late_fee";
        public const string FeeInvoiceRecalculate = "fee_invoice.recalculate";
        public const string FeeInvoiceBulkGenerate = "fee_invoice.bulk_generate";
        public const string FeeInvoiceExport = "fee_invoice.export";

        // Payments
        public const string FeePaymentRead = "fee_payment.read";
        public const string FeePaymentCollect = "fee_payment.collect";
        public const string FeePaymentAdvance = "fee_payment.advance";
        public const string FeePaymentApplyCredit = "fee_payment.apply_credit";
        public const string FeePaymentTransferCredit = "fee_payment.transfer_credit";
        public const string FeePaymentBulkCsv = "fee_payment.bulk_csv";
        public const string FeePaymentBounce = "fee_payment.bounce";
        public const string FeePaymentReceipt = "fee_payment.receipt";

        // Concessions
        public const string FeeConcessionRead = "fee_concession.read";
        public const string FeeConcessionCreate = "fee_concession.create";
        public const string FeeConcessionUpdate = "fee_concession.update";
        public const string FeeConcessionDelete = "fee_concession.delete";
        public const string FeeConcessionAllocate = "fee_concession.allocate";
        public const string FeeConcessionApprove = "fee_concession.approve";
        public const string FeeConcessionReject = "fee_concession.reject";
        public const string FeeConcessionAutoSibling = "fee_concession.auto_sibling";

        // Refunds
        public const string FeeRefundRead = "fee_refund.read";
        public const string FeeRefundRequest = "fee_refund.request";
        public const string FeeRefundApprove = "fee_refund.approve";
        public const string FeeRefundComplete = "fee_refund.complete";
        public const string FeeRefundReject = "fee_refund.reject";

        // Reports
        public const string FeeReportRead = "fee_report.read";
        public const string FeeReportExport = "fee_report.export";
        public const string FeeReportAuditLogs = "fee_report.audit_logs";

        // Parent Portal
        public const string FeePortalRead = "fee_portal.read";
        public const string FeePortalOptIn = "fee_portal.opt_in";

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
