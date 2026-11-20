using FluentValidation;

namespace SchoolERP.Application.Features.Fees.Commands.FeeStructure.CreateFeeStructure;

public sealed class CreateFeeStructureCommandValidator : AbstractValidator<CreateFeeStructureCommand>
{
    // snake_case enum values (must match EnumMaps_JsonAdapter / EnumMaps)
    private static readonly string[] LateFeeModeValues = { "none", "per_head", "whole_invoice" };
    private static readonly string[] InvoiceLateFeeTypeValues = { "stagnant_fixed", "graduated", "monthly" };

    public CreateFeeStructureCommandValidator()
    {
        RuleFor(x => x.Request.AcademicYearId).GreaterThan(0);
        RuleFor(x => x.Request.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Request.ApplicableFrom).LessThanOrEqualTo(x => x.Request.ApplicableTo)
            .WithMessage("ApplicableFrom must be before ApplicableTo.");

        RuleFor(x => x.Request)
            .Must(r => r.ClassId.HasValue || r.ClassGroupId.HasValue)
            .WithMessage("At least one of ClassId or ClassGroupId must be provided.");

        RuleFor(x => x.Request.Lines).NotEmpty().WithMessage("At least one fee line is required.");
        RuleForEach(x => x.Request.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.FeeHeadId).GreaterThan(0);
            line.RuleFor(l => l.Amount).GreaterThanOrEqualTo(0);
        });

        // ── Late fee (parent-level) ──
        RuleFor(x => x.Request.LateFeeMode)
            .Must(BeValidLateFeeMode)
            .WithMessage($"LateFeeMode must be one of: {string.Join(", ", LateFeeModeValues)}.");

        // Mode = whole_invoice → invoice-level fields required
        When(IsWholeInvoiceMode, () =>
        {
            RuleFor(x => x.Request.InvoiceLateFeeType)
                .NotEmpty()
                .Must(BeValidInvoiceLateFeeType)
                .WithMessage($"InvoiceLateFeeType must be one of: {string.Join(", ", InvoiceLateFeeTypeValues)}.");

            RuleFor(x => x.Request.InvoiceLateFeeValue)
                .NotNull().GreaterThanOrEqualTo(0m);

            RuleFor(x => x.Request.InvoiceLateFeeGraceDays)
                .NotNull().GreaterThanOrEqualTo((short)0);

            // Slab config required when type = graduated
            When(IsGraduatedType, () =>
            {
                RuleFor(x => x.Request.InvoiceLateFeeSlabConfig)
                    .NotEmpty().WithMessage("InvoiceLateFeeSlabConfig is required when InvoiceLateFeeType = 'graduated'.")
                    .Must(IsValidJson).WithMessage("InvoiceLateFeeSlabConfig must be valid JSON.");
            });

            // If slab config provided for non-graduated types, still validate JSON
            RuleFor(x => x.Request.InvoiceLateFeeSlabConfig)
                .Must(s => string.IsNullOrWhiteSpace(s) || IsValidJson(s))
                .WithMessage("InvoiceLateFeeSlabConfig must be valid JSON.")
                .When(x => !IsGraduatedType(x));
        });

        // Mode = none OR per_head → invoice-level fields must NOT be set (reject, not silently null)
        When(IsNoneOrPerHeadMode, () =>
        {
            RuleFor(x => x.Request.InvoiceLateFeeType)
                .Null().WithMessage("InvoiceLateFeeType must not be set when LateFeeMode is 'none' or 'per_head'.");
            RuleFor(x => x.Request.InvoiceLateFeeValue)
                .Null().WithMessage("InvoiceLateFeeValue must not be set when LateFeeMode is 'none' or 'per_head'.");
            RuleFor(x => x.Request.InvoiceLateFeeGraceDays)
                .Null().WithMessage("InvoiceLateFeeGraceDays must not be set when LateFeeMode is 'none' or 'per_head'.");
            RuleFor(x => x.Request.InvoiceLateFeeCapPercent)
                .Null().WithMessage("InvoiceLateFeeCapPercent must not be set when LateFeeMode is 'none' or 'per_head'.");
            RuleFor(x => x.Request.InvoiceLateFeeSlabConfig)
                .Null().WithMessage("InvoiceLateFeeSlabConfig must not be set when LateFeeMode is 'none' or 'per_head'.");
        });
    }

    // ── Helpers ──

    private static bool BeValidLateFeeMode(CreateFeeStructureCommand cmd, string value)
        => !string.IsNullOrWhiteSpace(value) &&
           Array.Exists(LateFeeModeValues, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static bool BeValidInvoiceLateFeeType(string? value)
        => !string.IsNullOrWhiteSpace(value) &&
           Array.Exists(InvoiceLateFeeTypeValues, v => v.Equals(value, StringComparison.OrdinalIgnoreCase));

    private static bool IsWholeInvoiceMode(CreateFeeStructureCommand cmd)
        => !string.IsNullOrWhiteSpace(cmd.Request.LateFeeMode) &&
           cmd.Request.LateFeeMode.Equals("whole_invoice", StringComparison.OrdinalIgnoreCase);

    private static bool IsNoneOrPerHeadMode(CreateFeeStructureCommand cmd)
        => !string.IsNullOrWhiteSpace(cmd.Request.LateFeeMode) &&
           (cmd.Request.LateFeeMode.Equals("none", StringComparison.OrdinalIgnoreCase) ||
            cmd.Request.LateFeeMode.Equals("per_head", StringComparison.OrdinalIgnoreCase));

    private static bool IsGraduatedType(CreateFeeStructureCommand cmd)
        => !string.IsNullOrWhiteSpace(cmd.Request.InvoiceLateFeeType) &&
           cmd.Request.InvoiceLateFeeType.Equals("graduated", StringComparison.OrdinalIgnoreCase);

    private static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}