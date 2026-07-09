using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncEdu.Core.Interfaces;
using SyncEdu.Shared.DTOs;
using System.Security.Claims;

namespace SyncEdu.Api.Controllers;

// ==================== 学习计划 ====================

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class LearningController : ControllerBase
{
    private readonly ILearningService _learningService;
    public LearningController(ILearningService learningService) => _learningService = learningService;

    /// <summary>获取小孩的学习计划列表</summary>
    [HttpGet("plans/{childId}")]
    public async Task<IActionResult> GetPlans(long childId)
    {
        var result = await _learningService.GetPlansAsync(childId);
        return Ok(result);
    }

    /// <summary>创建学习计划</summary>
    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateLearningPlanDto dto)
    {
        var result = await _learningService.CreatePlanAsync(dto);
        return Ok(result);
    }

    /// <summary>获取学习进度</summary>
    [HttpGet("progress/{planId}/{childId}")]
    public async Task<IActionResult> GetProgress(long planId, long childId)
    {
        var result = await _learningService.GetProgressAsync(planId, childId);
        return Ok(result);
    }

    /// <summary>更新学习进度</summary>
    [HttpPost("progress")]
    public async Task<IActionResult> UpdateProgress([FromBody] UpdateProgressDto dto)
    {
        var result = await _learningService.UpdateProgressAsync(dto);
        return Ok(result);
    }

    /// <summary>开始学习会话</summary>
    [HttpPost("session/start")]
    public async Task<IActionResult> StartSession([FromBody] StartSessionDto dto)
    {
        var result = await _learningService.StartSessionAsync(dto);
        return Ok(result);
    }

    /// <summary>结束学习会话</summary>
    [HttpPost("session/{sessionId}/end")]
    public async Task<IActionResult> EndSession(long sessionId)
    {
        var result = await _learningService.EndSessionAsync(sessionId);
        return Ok(result);
    }
}

// ==================== 打卡 ====================

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CheckInController : ControllerBase
{
    private readonly ICheckInService _checkInService;
    public CheckInController(ICheckInService checkInService) => _checkInService = checkInService;

    /// <summary>创建打卡记录</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCheckInDto dto)
    {
        var result = await _checkInService.CreateCheckInAsync(dto);
        return Ok(result);
    }

    /// <summary>获取打卡记录</summary>
    [HttpGet("{childId}")]
    public async Task<IActionResult> GetList(long childId, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var result = await _checkInService.GetCheckInsAsync(childId, startDate, endDate);
        return Ok(result);
    }

    /// <summary>获取打卡统计</summary>
    [HttpGet("stats/{childId}")]
    public async Task<IActionResult> GetStats(long childId)
    {
        var result = await _checkInService.GetStatsAsync(childId);
        return Ok(result);
    }
}

// ==================== 学习提醒 ====================

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReminderController : ControllerBase
{
    private readonly IReminderService _reminderService;
    public ReminderController(IReminderService reminderService) => _reminderService = reminderService;

    /// <summary>获取提醒列表</summary>
    [HttpGet("{childId}")]
    public async Task<IActionResult> GetList(long childId)
    {
        var result = await _reminderService.GetRemindersAsync(childId);
        return Ok(result);
    }

    /// <summary>创建提醒</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReminderDto dto)
    {
        var result = await _reminderService.CreateReminderAsync(dto);
        return Ok(result);
    }

    /// <summary>切换提醒开关</summary>
    [HttpPut("{reminderId}/toggle")]
    public async Task<IActionResult> Toggle(long reminderId, [FromQuery] bool isEnabled)
    {
        var result = await _reminderService.ToggleReminderAsync(reminderId, isEnabled);
        return Ok(result);
    }

    /// <summary>删除提醒</summary>
    [HttpDelete("{reminderId}")]
    public async Task<IActionResult> Delete(long reminderId)
    {
        var result = await _reminderService.DeleteReminderAsync(reminderId);
        return Ok(result);
    }
}

// ==================== 成就与激励 ====================

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AchievementController : ControllerBase
{
    private readonly IAchievementService _achievementService;
    public AchievementController(IAchievementService achievementService) => _achievementService = achievementService;

    /// <summary>获取所有成就定义</summary>
    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _achievementService.GetAllAchievementsAsync();
        return Ok(result);
    }

    /// <summary>获取小孩已获得的成就</summary>
    [HttpGet("child/{childId}")]
    public async Task<IActionResult> GetChildAchievements(long childId)
    {
        var result = await _achievementService.GetChildAchievementsAsync(childId);
        return Ok(result);
    }

    /// <summary>获取积分汇总</summary>
    [HttpGet("points/{childId}")]
    public async Task<IActionResult> GetPoints(long childId)
    {
        var result = await _achievementService.GetPointsSummaryAsync(childId);
        return Ok(result);
    }

    /// <summary>获取鼓励记录</summary>
    [HttpGet("encouragements/{childId}")]
    public async Task<IActionResult> GetEncouragements(long childId)
    {
        var result = await _achievementService.GetEncouragementsAsync(childId);
        return Ok(result);
    }

    /// <summary>发送鼓励</summary>
    [HttpPost("encouragements")]
    public async Task<IActionResult> AddEncouragement([FromBody] AddEncouragementDto dto)
    {
        var result = await _achievementService.AddEncouragementAsync(dto.ChildId, dto.Content, dto.Reason);
        return Ok(result);
    }

    /// <summary>奖励积分</summary>
    [HttpPost("points")]
    public async Task<IActionResult> AwardPoints([FromBody] AwardPointsDto dto)
    {
        var result = await _achievementService.AwardPointsAsync(dto.ChildId, dto.Points, dto.Reason);
        return Ok(result);
    }
}

// ==================== 教育资源同步 ====================

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EducationSyncController : ControllerBase
{
    private readonly IEducationSyncService _syncService;
    public EducationSyncController(IEducationSyncService syncService) => _syncService = syncService;

    /// <summary>获取同步状态</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _syncService.GetSyncStatusAsync();
        return Ok(result);
    }

    /// <summary>手动触发同步</summary>
    [HttpPost("sync")]
    public async Task<IActionResult> Sync()
    {
        var result = await _syncService.SyncEducationDataAsync();
        return Ok(result);
    }

    /// <summary>获取教材详情（含章节课时）</summary>
    [HttpGet("textbook/{textbookId}")]
    public async Task<IActionResult> GetTextbookDetail(long textbookId)
    {
        var result = await _syncService.GetTextbookDetailAsync(textbookId);
        return Ok(result);
    }

    /// <summary>获取学习推荐</summary>
    [HttpGet("recommendations/{childId}")]
    public async Task<IActionResult> GetRecommendations(long childId)
    {
        var result = await _syncService.GetRecommendationsAsync(childId);
        return Ok(result);
    }
}

// ==================== 辅助 DTO ====================

public class AddEncouragementDto
{
    public long ChildId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class AwardPointsDto
{
    public long ChildId { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
}
