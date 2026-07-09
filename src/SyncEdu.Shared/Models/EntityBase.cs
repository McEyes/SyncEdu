using SqlSugar;

namespace SyncEdu.Shared.Models;

/// <summary>
/// 实体基类
/// </summary>
public abstract class EntityBase
{
    /// <summary>主键</summary>
    [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
    public long Id { get; set; }

    /// <summary>创建时间</summary>
    [SugarColumn(IsNullable = false)]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>更新时间</summary>
    [SugarColumn(IsNullable = true)]
    public DateTime? UpdatedAt { get; set; }

    /// <summary>创建人</summary>
    [SugarColumn(IsNullable = true)]
    public long? CreatedBy { get; set; }

    /// <summary>是否删除</summary>
    [SugarColumn(IsNullable = false)]
    public bool IsDeleted { get; set; } = false;
}
