using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Enquiry.RestoreEnquiry;

public record RestoreEnquiryCommand(long Id) : ICommand<bool>;
