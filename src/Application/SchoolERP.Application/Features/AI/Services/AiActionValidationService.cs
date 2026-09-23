using SchoolERP.Application.Common.Interfaces;
using SchoolERP.Application.Features.AI.DTOs;
using SchoolERP.Application.Features.AI.Tools;

namespace SchoolERP.Application.Features.AI.Services;

public sealed class AiActionValidationService
{
    private readonly IAiToolRegistry _toolRegistry;
    private readonly IAiToolSchemaValidator _schemaValidator;

    public AiActionValidationService(
        IAiToolRegistry toolRegistry,
        IAiToolSchemaValidator schemaValidator)
    {
        _toolRegistry = toolRegistry;
        _schemaValidator = schemaValidator;
    }

    public AiActionValidationResult Validate(
        AiActionProposal proposal)
    {
        ArgumentNullException.ThrowIfNull(proposal);

        if (string.IsNullOrWhiteSpace(
                proposal.ActionName))
        {
            return AiActionValidationResult.Failure(
            [
                "AI action name is required."
            ]);
        }

        if (proposal.Version <= 0)
        {
            return AiActionValidationResult.Failure(
            [
                "AI action version must be greater than zero."
            ]);
        }

        var tool = _toolRegistry.Get(
            proposal.ActionName,
            proposal.Version);

        if (tool is null)
        {
            return AiActionValidationResult.Failure(
            [
                $"AI tool '{proposal.ActionName}' version '{proposal.Version}' is not registered."
            ]);
        }

        var schemaResult =
            _schemaValidator.Validate(
                tool,
                proposal.Arguments);

        if (!schemaResult.IsValid)
        {
            return AiActionValidationResult.Failure(
                schemaResult.Errors);
        }

        return AiActionValidationResult.Success(
            tool);
    }
}