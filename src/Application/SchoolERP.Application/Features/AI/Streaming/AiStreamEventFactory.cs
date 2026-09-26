using SchoolERP.Application.Features.AI.Confirmation;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Streaming;

public static class AiStreamEventFactory
{
    public static AiStreamEvent ToolProposed(
        string toolName,
        int version,
        object? arguments = null)
    {
        return new AiStreamEvent
        {
            Type = AiStreamEventType.ToolProposed,
            //Data = new
            //{
            //    ToolName = toolName,
            //    Version = version,
            //    Arguments = arguments
            //}
        };
    }

    public static AiStreamEvent ConfirmationRequired(
        AiConfirmationRequirement confirmation)
    {
        ArgumentNullException.ThrowIfNull(confirmation);

        return new AiStreamEvent
        {
            Type = AiStreamEventType.ConfirmationRequired,
            //Data = confirmation
        };
    }

    public static AiStreamEvent ToolExecutionStarted(
        string toolName,
        int version)
    {
        return new AiStreamEvent
        {
            Type = AiStreamEventType.ToolExecutionStarted,
            //Data = new
            //{
            //    ToolName = toolName,
            //    Version = version
            //}
        };
    }

    public static AiStreamEvent ToolExecutionCompleted(
        AiToolExecutionResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return new AiStreamEvent
        {
            Type = AiStreamEventType.ToolExecutionCompleted,
            //Data = result
        };
    }

    public static AiStreamEvent Error(
        string message)
    {
        return new AiStreamEvent
        {
            Type = AiStreamEventType.Error,
            Content = message
        };
    }
}