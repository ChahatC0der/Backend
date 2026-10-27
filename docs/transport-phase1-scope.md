# Transport Module — Phase 1 Scope

## In scope (Phase 1)

| Area | Delivered |
|---|---|
| Vehicles | CRUD + status transitions (active/maintenance/retired) + soft delete + restore + Excel export |
| Vehicle Documents | Upload/replace per type (insurance, puc, fitness, registration) — one per type per vehicle |
| Vehicle Maintenance | Append-only history (date, description, cost, odometer) |
| Routes | CRUD + active/inactive status + branch-scoped code uniqueness |
| Stops | CRUD + reorder (atomic) + stop type (pickup_only/drop_only/both) |
| Route-Vehicle Assignment | History-preserving (IsCurrent flip) — one current route per vehicle |
| Driver Assignment | History-preserving — one current driver per vehicle |
| Transport Fees | Route OR Stop level, per academic year, monthly/quarterly/half_yearly |
| Student Assignments | Manual + bulk CSV (cap 500) + history (IsCurrent) + capacity warning |
| Fees Integration | InvoiceBuilder extension — adds transport fee line from active assignment |
| Reports | Dashboard, student transport list, route-wise list, maintenance log, driver report, expiry alerts |
| Parent Portal | Parent-child transport view (route, stop, vehicle, pickup time) |

## Explicitly out of Phase 1

| Item | Reason |
|---|---|
| **Automated background jobs** | All flows are manual endpoints — admin triggers via HTTP |
| **Automated SMS/email notifications** | Expiry warnings + capacity warnings returned in API response; frontend decides delivery |
| GPS / live tracking | Phase 2 |
| Automatic route optimization | Phase 2 — stop ordering is manual in Phase 1 |
| Full fleet management (fuel ledger) | Phase 2 — only basic maintenance (cost, odometer) in Phase 1 |
| Transport-specific financial ledger | Fees module owns invoices/payments |
| Partial payment-linked refunds | N/A — Transport never creates payments |
| Transport-specific audit table | Uses existing FinancialAuditLogs pattern via TODO comments |

## Schema notes (verified via SSMS)

- All 9 tables use **Guid** for TenantId/BranchId
- All 9 tables use **IsDeleted BIT** (not DeletedAt) — opposite of Fees pattern
- **No** DeletedAt columns anywhere in transport tables
- Timestamp columns vary by table (see per-entity base mapping in Phase 1)
- 9 tables exist in DB: Vehicles, VehicleDocuments, VehicleMaintenance, Routes, Stops, RouteVehicles, DriverAssignments, TransportFees, TransportAssignments

## Locked business rules

1. Vehicle belongs to one branch — number unique per (TenantId, BranchId)
2. One vehicle can be current on only one route (filtered unique index)
3. One route can have multiple current vehicles (N:M junction)
4. One vehicle can have only one current driver (filtered unique index)
5. Transport fee: RouteId XOR StopId (exactly one, validated in handler)
6. Transport fee frequency: monthly, quarterly, half_yearly only (per DB check constraint)
7. Student has one current assignment per (StudentId, AcademicYearId)
8. Mid-month stop change → this month invoice unchanged, new fee from next billing cycle
9. Transport fee head code = "TRANSPORT" — reused from Fees module (seeded in Phase 1)
10. Capacity overflow = warning + admin override (not hard block)
11. Pickup ≠ drop route = warning in response (no notification)
12. All flows manual — no background jobs, no scheduled notifications

## Deferred to Phase 2

- GPS integration & delay notifications
- Automatic route optimization
- Fuel consumption tracking
- Auto-triggered SMS/email notifications for expiry or capacity
- Hangfire jobs for any transport domain task

## Enum DB strings (locked)

| Enum | DB Values |
|---|---|
| VehicleType | bus, van, minibus, car, other |
| VehicleStatus | active, maintenance, retired |
| VehicleDocumentType | insurance, puc, fitness, registration |
| RouteStatus | active, inactive |
| StopType | pickup_only, drop_only, both |
| TransportFeeFrequency | monthly, quarterly, half_yearly |
