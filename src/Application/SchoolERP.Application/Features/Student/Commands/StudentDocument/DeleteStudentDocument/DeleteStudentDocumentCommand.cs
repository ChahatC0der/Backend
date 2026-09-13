using SchoolERP.Application.Common.Abstractions;

public record DeleteStudentDocumentCommand(long Id) : ICommand<bool>;