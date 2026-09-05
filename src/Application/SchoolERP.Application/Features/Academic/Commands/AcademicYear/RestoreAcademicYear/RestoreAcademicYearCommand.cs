using SchoolERP.Application.Common.Abstractions;

public record RestoreAcademicYearCommand(long Id) : ICommand<bool>;