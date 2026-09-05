using SchoolERP.Application.Common.Abstractions;

public record DeleteAcademicYearCommand(long Id) : ICommand<bool>;