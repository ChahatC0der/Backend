using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Queries.Vehicle.GetVehicleDocuments;

public sealed record GetVehicleDocumentsQuery(long VehicleId) : IQuery<IReadOnlyList<VehicleDocumentResponse>>;
