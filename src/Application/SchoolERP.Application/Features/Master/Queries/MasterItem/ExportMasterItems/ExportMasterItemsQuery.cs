using SchoolERP.Application.Common.Abstractions;

public record ExportMasterItemsQuery(long CategoryId) : IQuery<byte[]>;