using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Transport.DTOs.VehicleDocument;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Transport.Commands.VehicleDocument.UploadVehicleDocument;

public record UploadVehicleDocumentCommand(long VehicleId, UploadVehicleDocumentRequest Request) : ICommand<VehicleDocumentResponse>;
