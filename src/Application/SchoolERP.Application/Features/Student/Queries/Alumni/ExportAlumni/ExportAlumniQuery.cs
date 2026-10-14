using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Alumni.ExportAlumni;

public record ExportAlumniQuery : IQuery<byte[]>;
