using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Student.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Student.Commands.Registration.RecordRegistrationFee;

public record RecordRegistrationFeeCommand(RecordRegistrationFeeRequest Request) : ICommand<RegistrationResponse>;
