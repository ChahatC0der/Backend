using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Auth.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Auth.Queries.GetMe;

// No Validator needed for this query, sirf UserId chahiye jo JWT se aayega
public record GetMeQuery(long UserId) : IQuery<GetMeResponse>;