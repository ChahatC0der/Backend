using MediatR;
using SchoolERP.Application.Common.Abstractions;
using SchoolERP.Application.Features.Master.DTOs;
using SchoolERP.Domain.Shared.Results;

namespace SchoolERP.Application.Features.Master.Commands.MasterCategory.CreateMasterCategory;

public record CreateMasterCategoryCommand(CreateMasterCategoryRequest Request) : ICommand<MasterCategoryResponse>;