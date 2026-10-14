using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Admission.ExportAdmissions;

public record ExportAdmissionsQuery : IQuery<byte[]>;
