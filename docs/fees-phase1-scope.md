# Fees & Concession — Phase 1 Scope

## In scope (Phase 1)

| Area | Delivered |
|---|---|
| Fee Heads | Full CRUD + seed defaults + soft delete + restore + bulk delete + Excel export |
| Fee Structures | CRUD + lines + clone + patch-status + assign-student + resolution chain |
| Concessions | CRUD + allocate + approve/reject + sibling auto-apply |
| Invoices | Generate / bulk-generate (500 cap) / regenerate / add line / adjust / waive late fee / recalculate / cancel |
| Payments | Collect / advance / apply-credit / transfer-credit / bulk-csv (500 cap) / cheque-bounce |
| Wallet | Append-only StudentCreditLedger + O(1) balance |
| Refunds | Wallet refunds (partial OK) + payment-linked refunds (FULL ONLY) |
| Reports | Dashboard / outstanding / defaulters / collection / concessions / credit balances / student ledger / audit logs — all with csv/xlsx export |
| Parent Portal | Child fee view + available optional fees + opt-in |

## Deferred to Phase 2

| Item | Reason |
|---|---|
| Payment gateway integration | Out of Phase 1 tables |
| Automated payment reconciliation | Requires gateway |
| Reminder events (email / SMS) | No notification infra in scope |
| Registration / admission fee auto-generation | No source data model in Phase 1 |
| Transport route-level proration | Only `ProrationMethod.CalendarDays` in Phase 1 |
| Credit expiry job | `TransactionType.Expired` reserved; job deferred |
| **Receipt PDF** | Phase 1 returns structured JSON only |
| Cheque clearing workflow (`ClearChequeCommand`) | Phase 1 = record-and-reverse |
| Bulk >500 records per request | Requires `TransactionBehavior` redesign |
| **Partial payment-linked refunds** | Needs `PaymentRefundAllocations` table |
| **Term/month-level opt-in re-registration** | Phase 1 = one per (student, AY, fee head) |

## Business rules locked

1. No tax calculation in Phase 1.
2. No `InvoiceType` column — multiple fee heads become separate `InvoiceLines`.
3. Academic Year only — no financial year filter.
4. Invoice/receipt numbering: atomic via `dbo.InvoiceSequences` (UPDLOCK). Uniqueness per `(TenantId, BranchId)` — NOT global. Formats: `{BranchCode}-{AYShort}-{seq:D6}` / `{BranchCode}-RCP-{AYShort}-{seq:D6}`.
5. Fee structure resolution chain: override → section → class → classgroup.
6. Duplicate invoice prevention: `UQ_Invoices_StudentYearPeriod` filtered on `Status != cancelled`.
7. Manual payment allocation only (`PaymentAllocationPriority = manual_only` default). Excess → wallet.
8. Concession stacking: percentage first, then fixed, cap at line amount.
9. Rounding: `MidpointRounding.AwayFromZero` @ 2dp via `Money.Round`.
10. Immutable financial records — no hard deletes on Payments / Invoices / Refunds; reversals only.
11. **Late fee represented as separate `InvoiceLine`** with `IsLateFeeLine = true` and `SourceInvoiceLineId` FK.
12. **Late-fee cap is per-fee-head** — applied against `InvoiceLine.LateFeeCapBase` (frozen at generation).
13. **Late-fee recalc is idempotent per invoice per day** via `UQ_LateFeeLogs_Invoice_Daily`.
14. **Audit log writes deferred** — entities + configs exist; `IFinancialAuditWriter` interface exists; no Infrastructure implementation; handler calls are commented `// TODO (audit)`.
