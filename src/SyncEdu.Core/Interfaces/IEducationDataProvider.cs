using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
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
/// 国家智慧教育平台数据提供者
/// 通过配置 ApiUrl 对接真实 API，未配置时使用内置模拟数据
/// </summary>
public class SmartEducationPlatformProvider : IEducationDataProvider
{
    private readonly HttpClient? _httpClient;
    private readonly string? _apiUrl;

    public SmartEducationPlatformProvider(IConfiguration configuration)
    {
        _apiUrl = configuration["EducationApi:SmartPlatformUrl"];
        if (!string.IsNullOrEmpty(_apiUrl))
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_apiUrl) };
        }
    }

    public string ProviderName => "国家智慧教育平台";

    public async Task<bool> IsAvailableAsync()
    {
        if (_httpClient == null) return true; // 使用内置数据时始终可用
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public Task<List<EducationStageDto>> SyncStagesAsync()
    {
        // 如果配置了真实API地址，调用远程接口
        if (_httpClient != null) return SyncStagesFromApiAsync();

        // 否则返回内置模拟数据
        var stages = new List<EducationStageDto>
        {
            new() { Id = 1, Name = "幼儿园", SortOrder = 1, Grades = new()
            {
                new() { Id = 1, Name = "小班", StageId = 1, SortOrder = 1 },
                new() { Id = 2, Name = "中班", StageId = 1, SortOrder = 2 },
                new() { Id = 3, Name = "大班", StageId = 1, SortOrder = 3 }
            }},
            new() { Id = 2, Name = "小学", SortOrder = 2, Grades = new()
            {
                new() { Id = 4, Name = "一年级", StageId = 2, SortOrder = 1 },
                new() { Id = 5, Name = "二年级", StageId = 2, SortOrder = 2 },
                new() { Id = 6, Name = "三年级", StageId = 2, SortOrder = 3 },
                new() { Id = 7, Name = "四年级", StageId = 2, SortOrder = 4 },
                new() { Id = 8, Name = "五年级", StageId = 2, SortOrder = 5 },
                new() { Id = 9, Name = "六年级", StageId = 2, SortOrder = 6 }
            }},
            new() { Id = 3, Name = "初中", SortOrder = 3, Grades = new()
            {
                new() { Id = 10, Name = "初一", StageId = 3, SortOrder = 1 },
                new() { Id = 11, Name = "初二", StageId = 3, SortOrder = 2 },
                new() { Id = 12, Name = "初三", StageId = 3, SortOrder = 3 }
            }},
            new() { Id = 4, Name = "高中", SortOrder = 4, Grades = new()
            {
                new() { Id = 13, Name = "高一", StageId = 4, SortOrder = 1 },
                new() { Id = 14, Name = "高二", StageId = 4, SortOrder = 2 },
                new() { Id = 15, Name = "高三", StageId = 4, SortOrder = 3 }
            }}
        };
        return Task.FromResult(stages);
    }

    public Task<List<GradeDto>> SyncGradesAsync(long stageId)
    {
        if (_httpClient != null) return SyncGradesFromApiAsync(stageId);

        var allStages = SyncStagesAsync().Result;
        var stage = allStages.FirstOrDefault(s => s.Id == stageId);
        return Task.FromResult(stage?.Grades ?? new List<GradeDto>());
    }

    public Task<List<TextbookVersionDto>> SyncTextbookVersionsAsync()
    {
        if (_httpClient != null) return SyncVersionsFromApiAsync();

        var versions = new List<TextbookVersionDto>
        {
            new() { Id = 1, Name = "人教版", Publisher = "人民教育出版社" },
            new() { Id = 2, Name = "北师大版", Publisher = "北京师范大学出版社" },
            new() { Id = 3, Name = "苏教版", Publisher = "江苏教育出版社" },
            new() { Id = 4, Name = "沪教版", Publisher = "上海教育出版社" },
            new() { Id = 5, Name = "外研版", Publisher = "外语教学与研究出版社" },
            new() { Id = 6, Name = "教科版", Publisher = "教育科学出版社" },
            new() { Id = 7, Name = "冀教版", Publisher = "河北教育出版社" },
            new() { Id = 8, Name = "鲁教版", Publisher = "山东教育出版社" }
        };
        return Task.FromResult(versions);
    }

    public Task SyncTextbookContentAsync(long textbookId)
    {
        // 模拟同步教材内容 - 实际应调用远程API
        return Task.CompletedTask;
    }

    // ===== 远程API调用方法（预留） =====

    private async Task<List<EducationStageDto>> SyncStagesFromApiAsync()
    {
        try
        {
            var result = await _httpClient!.GetFromJsonAsync<List<EducationStageDto>>("/api/stages");
            return result ?? new List<EducationStageDto>();
        }
        catch
        {
            return new List<EducationStageDto>();
        }
    }

    private async Task<List<GradeDto>> SyncGradesFromApiAsync(long stageId)
    {
        try
        {
            var result = await _httpClient!.GetFromJsonAsync<List<GradeDto>>($"/api/grades/{stageId}");
            return result ?? new List<GradeDto>();
        }
        catch
        {
            return new List<GradeDto>();
        }
    }

    private async Task<List<TextbookVersionDto>> SyncVersionsFromApiAsync()
    {
        try
        {
            var result = await _httpClient!.GetFromJsonAsync<List<TextbookVersionDto>>("/api/textbook-versions");
            return result ?? new List<TextbookVersionDto>();
        }
        catch
        {
            return new List<TextbookVersionDto>();
        }
    }
}

/// <summary>
/// 地方教育局数据提供者
/// 支持配置地方教育局API地址
/// </summary>
public class LocalEducationBureauProvider : IEducationDataProvider
{
    private readonly HttpClient? _httpClient;
    private readonly string? _apiUrl;

    public LocalEducationBureauProvider(IConfiguration configuration)
    {
        _apiUrl = configuration["EducationApi:LocalBureauUrl"];
        if (!string.IsNullOrEmpty(_apiUrl))
        {
            _httpClient = new HttpClient { BaseAddress = new Uri(_apiUrl) };
        }
    }

    public string ProviderName => "地方教育局";

    public async Task<bool> IsAvailableAsync()
    {
        if (_httpClient == null) return false; // 未配置地址时不可用
        try
        {
            var response = await _httpClient.GetAsync("/health");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<List<EducationStageDto>> SyncStagesAsync()
    {
        if (_httpClient == null) return new List<EducationStageDto>();
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<EducationStageDto>>("/api/stages");
            return result ?? new List<EducationStageDto>();
        }
        catch
        {
            return new List<EducationStageDto>();
        }
    }

    public async Task<List<GradeDto>> SyncGradesAsync(long stageId)
    {
        if (_httpClient == null) return new List<GradeDto>();
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<GradeDto>>($"/api/grades/{stageId}");
            return result ?? new List<GradeDto>();
        }
        catch
        {
            return new List<GradeDto>();
        }
    }

    public async Task<List<TextbookVersionDto>> SyncTextbookVersionsAsync()
    {
        if (_httpClient == null) return new List<TextbookVersionDto>();
        try
        {
            var result = await _httpClient.GetFromJsonAsync<List<TextbookVersionDto>>("/api/textbook-versions");
            return result ?? new List<TextbookVersionDto>();
        }
        catch
        {
            return new List<TextbookVersionDto>();
        }
    }

    public Task SyncTextbookContentAsync(long textbookId)
    {
        // TODO: 对接地方教育局教材内容API
        return Task.CompletedTask;
    }
}
