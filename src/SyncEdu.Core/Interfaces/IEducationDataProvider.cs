using SyncEdu.Shared.DTOs;

namespace SyncEdu.Core.Interfaces;

/// <summary>
/// 教育数据提供者抽象接口 - 用于对接不同教育局/教育平台API
/// </summary>
public interface IEducationDataProvider
{
    /// <summary>提供者名称</summary>
    string ProviderName { get; }

    /// <summary>是否可用</summary>
    Task<bool> IsAvailableAsync();

    /// <summary>同步教育阶段数据</summary>
    Task<List<EducationStageDto>> SyncStagesAsync();

    /// <summary>同步年级数据</summary>
    Task<List<GradeDto>> SyncGradesAsync(long stageId);

    /// <summary>同步教材版本</summary>
    Task<List<TextbookVersionDto>> SyncTextbookVersionsAsync();

    /// <summary>同步教材内容（章节、课时）</summary>
    Task SyncTextbookContentAsync(long textbookId);
}

/// <summary>
/// 国家智慧教育平台数据提供者（预留实现）
/// </summary>
public class SmartEducationPlatformProvider : IEducationDataProvider
{
    public string ProviderName => "国家智慧教育平台";

    public Task<bool> IsAvailableAsync()
    {
        // TODO: 检查API连通性
        return Task.FromResult(false);
    }

    public Task<List<EducationStageDto>> SyncStagesAsync()
    {
        // TODO: 对接国家智慧教育平台API获取阶段数据
        return Task.FromResult(new List<EducationStageDto>());
    }

    public Task<List<GradeDto>> SyncGradesAsync(long stageId)
    {
        // TODO: 对接API获取年级数据
        return Task.FromResult(new List<GradeDto>());
    }

    public Task<List<TextbookVersionDto>> SyncTextbookVersionsAsync()
    {
        // TODO: 对接API获取教材版本
        return Task.FromResult(new List<TextbookVersionDto>());
    }

    public Task SyncTextbookContentAsync(long textbookId)
    {
        // TODO: 对接API同步教材章节和课时内容
        return Task.CompletedTask;
    }
}

/// <summary>
/// 地方教育局数据提供者（预留实现）
/// </summary>
public class LocalEducationBureauProvider : IEducationDataProvider
{
    public string ProviderName => "地方教育局";

    public Task<bool> IsAvailableAsync()
    {
        return Task.FromResult(false);
    }

    public Task<List<EducationStageDto>> SyncStagesAsync()
    {
        return Task.FromResult(new List<EducationStageDto>());
    }

    public Task<List<GradeDto>> SyncGradesAsync(long stageId)
    {
        return Task.FromResult(new List<GradeDto>());
    }

    public Task<List<TextbookVersionDto>> SyncTextbookVersionsAsync()
    {
        return Task.FromResult(new List<TextbookVersionDto>());
    }

    public Task SyncTextbookContentAsync(long textbookId)
    {
        return Task.CompletedTask;
    }
}
