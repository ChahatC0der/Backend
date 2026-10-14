using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Registration.ExportRegistrations;

public record ExportRegistrationsQuery : IQuery<byte[]>;
