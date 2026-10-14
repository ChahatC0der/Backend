using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.RestoreEnquiryFollowup;

public record RestoreEnquiryFollowupCommand(long Id) : ICommand<bool>;
