using SchoolERP.Application.Features.AI.DTOs;

namespace SchoolERP.Application.Common.Interfaces;

public interface IAiExecutionContextAccessor
{
    AiExecutionContext GetCurrent();
}