namespace SchoolERP.Application.Features.AI.Streaming;

public enum AiStreamEventType
{
    MessageStarted = 1,
    MessageDelta = 2,
    ToolProposed = 3,
    ConfirmationRequired = 4,
    ToolExecutionStarted = 5,
    ToolExecutionCompleted = 6,
    MessageCompleted = 7,
    Error = 8
}