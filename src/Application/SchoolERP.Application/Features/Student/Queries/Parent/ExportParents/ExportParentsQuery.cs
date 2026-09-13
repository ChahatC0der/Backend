using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
public record ExportParentsQuery(long? StudentId = null) : IQuery<byte[]>;
