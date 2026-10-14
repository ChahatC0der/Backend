using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Queries.Family.ExportFamilies;

public record ExportFamiliesQuery : IQuery<byte[]>;
