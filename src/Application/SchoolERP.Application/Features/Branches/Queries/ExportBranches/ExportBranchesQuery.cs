using MediatR;
using SchoolERP.Application.Common.Abstractions;

namespace SchoolERP.Application.Features.Branches.Queries.ExportBranches;

public record ExportBranchesQuery() : IQuery<byte[]>;