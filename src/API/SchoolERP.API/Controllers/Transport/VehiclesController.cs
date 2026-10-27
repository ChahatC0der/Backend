using MediatR;
using Microsoft.AspNetCore.Mvc;
using SchoolERP.API.Authorization;
using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.CreateVehicle;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.DeleteVehicle;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.PatchVehicleStatus;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.RestoreVehicle;
using SchoolERP.Application.Features.Transport.Commands.Vehicle.UpdateVehicle;
using SchoolERP.Application.Features.Transport.Commands.VehicleDocument.UploadVehicleDocument;
using SchoolERP.Application.Features.Transport.Commands.VehicleMaintenance.AddVehicleMaintenance;
using SchoolERP.Application.Features.Transport.DTOs.Vehicle;
using SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;
using SchoolERP.Application.Features.Transport.DTOs.VehicleMaintenance;
using SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleById;
using SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleDocuments;
using SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleMaintenance;
using SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicles;
using SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehiclesLight;
using SchoolERP.Domain.Shared.Permissions;

namespace SchoolERP.API.Controllers.Transport;

[ApiController]
[Route("api/transport/vehicles")]
public sealed class VehiclesController : BaseApiController
{
    private readonly IMediator _mediator;
    private readonly IExcelExportService _excel;
    public VehiclesController(IMediator mediator, IExcelExportService excel)
    { _mediator = mediator; _excel = excel; }

    // ----- Reads -----

    [HttpGet]
    [HasPermission(PermissionNames.TransportVehicleRead)]
    public async Task<IActionResult> Get([FromQuery] GetVehiclesQuery query, CancellationToken ct)
        => HandleResult(await _mediator.Send(query, ct));

    [HttpGet("light")]
    [HasPermission(PermissionNames.TransportVehicleRead)]
    public async Task<IActionResult> GetLight(CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetVehiclesLightQuery(), ct));

    [HttpGet("{id:long}")]
    [HasPermission(PermissionNames.TransportVehicleRead)]
    public async Task<IActionResult> GetById([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetVehicleByIdQuery(id), ct));

    [HttpGet("{id:long}/documents")]
    //[HasPermission(PermissionNames.TransportVehicleDocumentRead)]
    public async Task<IActionResult> GetDocuments([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetVehicleDocumentsQuery(id), ct));

    [HttpGet("{id:long}/maintenance")]
    //[HasPermission(PermissionNames.TransportVehicleMaintenanceRead)]
    public async Task<IActionResult> GetMaintenance([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new GetVehicleMaintenanceQuery(id), ct));

    // ----- Writes -----

    [HttpPost]
    //[HasPermission(PermissionNames.TransportVehicleCreate)]
    public async Task<IActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new CreateVehicleCommand(request), ct));

    [HttpPut("{id:long}")]
    //[HasPermission(PermissionNames.TransportVehicleUpdate)]
    public async Task<IActionResult> Update([FromRoute] long id, [FromBody] UpdateVehicleRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UpdateVehicleCommand(id, request), ct));

    [HttpPatch("{id:long}/status")]
    //[HasPermission(PermissionNames.TransportVehicleUpdate)]
    public async Task<IActionResult> PatchStatus([FromRoute] long id, [FromBody] PatchVehicleStatusRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new PatchVehicleStatusCommand(id, request), ct));

    [HttpDelete("{id:long}")]
    //[HasPermission(PermissionNames.TransportVehicleDelete)]
    public async Task<IActionResult> Delete([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new DeleteVehicleCommand(id), ct));

    [HttpPost("{id:long}/restore")]
    //[HasPermission(PermissionNames.TransportVehicleRestore)]
    public async Task<IActionResult> Restore([FromRoute] long id, CancellationToken ct)
        => HandleResult(await _mediator.Send(new RestoreVehicleCommand(id), ct));

    [HttpPost("{id:long}/documents")]
    //[HasPermission(PermissionNames.TransportVehicleDocumentUpload)]
    public async Task<IActionResult> UploadDocument([FromRoute] long id, [FromBody] UploadVehicleDocumentRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new UploadVehicleDocumentCommand(id, request), ct));

    [HttpPost("{id:long}/maintenance")]
    //[HasPermission(PermissionNames.TransportVehicleMaintenanceCreate)]
    public async Task<IActionResult> AddMaintenance([FromRoute] long id, [FromBody] CreateVehicleMaintenanceRequest request, CancellationToken ct)
        => HandleResult(await _mediator.Send(new AddVehicleMaintenanceCommand(id, request), ct));

    // ----- Export -----

    [HttpGet("export")]
    //[HasPermission(PermissionNames.TransportVehicleExport)]
    public async Task<IActionResult> Export([FromQuery] GetVehiclesQuery query, [FromQuery] string? format, CancellationToken ct)
    {
        query.Page = 1; query.Size = 200;
        var r = await _mediator.Send(query, ct);
        if (r.IsFailure) return HandleResult(r);

        var rows = r.Value.Data.Select(v => new VehicleExportRow
        {
            Id = v.Id,
            VehicleNumber = v.VehicleNumber,
            VehicleType = v.VehicleType,
            Capacity = v.Capacity,
            Make = v.Make,
            Model = v.Model,
            Year = v.Year,
            InsuranceValidUntil = v.InsuranceValidUntil,
            PucValidUntil = v.PucValidUntil,
            FitnessValidUntil = v.FitnessValidUntil,
            Status = v.Status,
            CreatedAt = v.CreatedAt,
        }).ToList();

        if (string.Equals(format, "csv", StringComparison.OrdinalIgnoreCase))
        {
            var csv = BuildCsv(rows);
            return File(csv, "text/csv", $"Vehicles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
        }

        var bytes = _excel.Export(rows, "Vehicles");
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Vehicles_{DateTime.UtcNow:yyyyMMdd_HHmmss}.xlsx");
    }

    private static byte[] BuildCsv(IReadOnlyList<VehicleExportRow> rows)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Id,VehicleNumber,VehicleType,Capacity,Make,Model,Year,InsuranceValidUntil,PucValidUntil,FitnessValidUntil,Status,CreatedAt");
        foreach (var r in rows)
        {
            sb.AppendLine($"{r.Id},\"{Esc(r.VehicleNumber)}\",{r.VehicleType},{r.Capacity},\"{Esc(r.Make)}\",\"{Esc(r.Model)}\",{r.Year?.ToString() ?? ""},{r.InsuranceValidUntil?.ToString("yyyy-MM-dd") ?? ""},{r.PucValidUntil?.ToString("yyyy-MM-dd") ?? ""},{r.FitnessValidUntil?.ToString("yyyy-MM-dd") ?? ""},{r.Status},{r.CreatedAt:yyyy-MM-dd HH:mm:ss}");
        }
        return System.Text.Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static string Esc(string? s) => (s ?? "").Replace("\"", "\"\"");
}
