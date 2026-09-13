using SchoolERP.Application.Common.Abstractions;

public record RestoreStudentDocumentCommand(long Id) : ICommand<bool>;