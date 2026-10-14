using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.EnquiryFollowup.UpdateEnquiryFollowup;

public record UpdateEnquiryFollowupCommand(UpdateEnquiryFollowupRequest Request) : ICommand<EnquiryFollowupResponse>;
