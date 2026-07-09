using SyncEdu.Core.Entities;
using SyncEdu.Core.Interfaces;
using SqlSugar;

namespace SyncEdu.Job;

/// <summary>
/// 教育数据同步定时任务
/// 定期从教育数据提供者同步最新的教育阶段、年级、教材版本等数据
/// </summary>
public class EducationSyncWorker : BackgroundService
{
    private readonly ILogger<EducationSyncWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _syncInterval = TimeSpan.FromHours(24); // 每天同步一次

    public EducationSyncWorker(
        ILogger<EducationSyncWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("教育数据同步Worker启动，同步间隔: {Interval}", _syncInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DoSyncAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "教育数据同步失败");
            }

            await Task.Delay(_syncInterval, stoppingToken);
        }
    }

    private async Task DoSyncAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var providers = scope.ServiceProvider.GetServices<IEducationDataProvider>();
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        _logger.LogInformation("开始教育数据同步，共 {Count} 个数据提供者", providers.Count());

        foreach (var provider in providers)
        {
            if (stoppingToken.IsCancellationRequested) break;

            var isAvailable = await provider.IsAvailableAsync();
            if (!isAvailable)
            {
                _logger.LogInformation("数据提供者 [{Name}] 不可用，跳过", provider.ProviderName);
                continue;
            }

            _logger.LogInformation("开始从 [{Name}] 同步数据", provider.ProviderName);

            try
            {
                // 同步教育阶段
                var stages = await provider.SyncStagesAsync();
                foreach (var stageDto in stages)
                {
                    var exists = await db.Queryable<EducationStage>()
                        .AnyAsync(s => s.Name == stageDto.Name && !s.IsDeleted);
                    if (!exists)
                    {
                        await db.Insertable(new EducationStage
                        {
                            Name = stageDto.Name,
                            SortOrder = stageDto.SortOrder,
                            CreatedAt = DateTime.UtcNow
                        }).ExecuteReturnIdentityAsync();
                    }
                }
                _logger.LogInformation("[{Name}] 同步了 {Count} 个教育阶段", provider.ProviderName, stages.Count);

                // 同步教材版本
                var versions = await provider.SyncTextbookVersionsAsync();
                foreach (var versionDto in versions)
                {
                    var exists = await db.Queryable<TextbookVersion>()
                        .AnyAsync(v => v.Name == versionDto.Name && !v.IsDeleted);
                    if (!exists)
                    {
                        await db.Insertable(new TextbookVersion
                        {
                            Name = versionDto.Name,
                            Publisher = versionDto.Publisher,
                            CreatedAt = DateTime.UtcNow
                        }).ExecuteReturnIdentityAsync();
                    }
                }
                _logger.LogInformation("[{Name}] 同步了 {Count} 个教材版本", provider.ProviderName, versions.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[{Name}] 同步过程中发生错误", provider.ProviderName);
            }
        }

        _logger.LogInformation("教育数据同步完成");
    }
}

/// <summary>
/// 提醒推送Worker - 检查到期的学习提醒并推送通知
/// </summary>
public class ReminderPushWorker : BackgroundService
{
    private readonly ILogger<ReminderPushWorker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    public ReminderPushWorker(
        ILogger<ReminderPushWorker> logger,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("提醒推送Worker启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "提醒检查失败");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckRemindersAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ISqlSugarClient>();

        var now = DateTime.UtcNow;
        var currentTime = new TimeSpan(now.Hour, now.Minute, 0);

        // 查找5分钟内到期的提醒
        var reminders = await db.Queryable<StudyReminder>()
            .Where(r => r.IsEnabled && !r.IsDeleted && r.ReminderTime != null)
            .ToListAsync();

        var dueReminders = reminders.Where(r =>
        {
            if (r.ReminderTime == null) return false;
            var diff = Math.Abs((r.ReminderTime.Value - currentTime).TotalMinutes);
            return diff <= 5;
        }).ToList();

        if (dueReminders.Count > 0)
        {
            _logger.LogInformation("发现 {Count} 个到期提醒", dueReminders.Count);
            // TODO: 实际推送逻辑（微信模板消息/App Push/短信等）
            foreach (var reminder in dueReminders)
            {
                _logger.LogInformation("提醒: [{Title}] 给小孩 {ChildId}", reminder.Title, reminder.ChildId);
            }
        }
    }
}
