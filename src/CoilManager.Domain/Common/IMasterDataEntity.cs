namespace CoilManager.Domain.Common;

public interface IMasterDataEntity
{
    string Code { get; }
    string Name { get; }
    string? Description { get; }
    bool IsActive { get; }
    byte[] RowVersion { get; }
}
