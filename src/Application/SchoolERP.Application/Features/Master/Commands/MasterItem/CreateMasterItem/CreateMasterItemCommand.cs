using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterItem.CreateMasterItem;

public record CreateMasterItemCommand(CreateMasterItemRequest Request) : ICommand<MasterItemResponse>;