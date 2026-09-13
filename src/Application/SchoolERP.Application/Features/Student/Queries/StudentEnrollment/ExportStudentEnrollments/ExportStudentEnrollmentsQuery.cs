using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record ExportStudentEnrollmentsQuery(long? StudentId = null) : IQuery<byte[]>;
