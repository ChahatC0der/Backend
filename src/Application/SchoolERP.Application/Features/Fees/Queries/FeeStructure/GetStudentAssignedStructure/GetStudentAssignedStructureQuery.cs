using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Fees.DTOs.FeeStructure;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Fees.Queries.FeeStructure.GetStudentAssignedStructure;

public sealed record GetStudentAssignedStructureQuery(long StudentId, long AcademicYearId) : IQuery<StudentAssignedStructureResponse>;
