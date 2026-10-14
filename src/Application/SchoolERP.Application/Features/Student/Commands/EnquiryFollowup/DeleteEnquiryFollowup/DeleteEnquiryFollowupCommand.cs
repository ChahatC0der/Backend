using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.DeleteEnquiryFollowup;

public record DeleteEnquiryFollowupCommand(long Id) : ICommand<bool>;
