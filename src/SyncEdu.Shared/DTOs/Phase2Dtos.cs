using System.ComponentModel.DataAnnotations;

namespace SyncEdu.Shared.DTOs;

// ==================== 学习计划 ====================

public class LearningPlanDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public long TextbookId { get; set; }
    public string TextbookTitle { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string GradeName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string SemesterName => Semester == 1 ? "上册" : "下册";
    public string Title { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ProgressPercent { get; set; }
    public int TotalLessons { get; set; }
    public int CompletedLessons { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLearningPlanDto
{
    [Required]
    public long ChildId { get; set; }

    /// <summary>年级ID</summary>
    [Required]
    public long GradeId { get; set; }

    /// <summary>科目ID</summary>
    [Required]
    public long SubjectId { get; set; }

    /// <summary>教材版本ID</summary>
    [Required]
    public long TextbookVersionId { get; set; }

    /// <summary>学期 1=上册 2=下册</summary>
    [Required]
    public int Semester { get; set; }

    /// <summary>快捷时长(天): 7, 21, 30, 90  与 StartDate/EndDate 二选一</summary>
    public int? DurationDays { get; set; }

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

// ==================== 学习进度 ====================

public class LearningProgressDto
{
    public long Id { get; set; }
    public long PlanId { get; set; }
    public long LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string ChapterTitle { get; set; } = string.Empty;
    public int Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? StudyMinutes { get; set; }
}

public class UpdateProgressDto
{
    [Required]
    public long PlanId { get; set; }

    [Required]
    public long LessonId { get; set; }

    [Required]
    public long ChildId { get; set; }

    /// <summary>1=未开始 2=学习中 3=已完成</summary>
    [Required]
    public int Status { get; set; }

    public int? StudyMinutes { get; set; }
}

// ==================== 学习会话 ====================

public class StudySessionDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public long? PlanId { get; set; }
    public long? LessonId { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int DurationMinutes { get; set; }
}

public class StartSessionDto
{
    [Required]
    public long ChildId { get; set; }
    public long? PlanId { get; set; }
    public long? LessonId { get; set; }
}

// ==================== 打卡 ====================

public class CheckInDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public string ChildName { get; set; } = string.Empty;
    public long? PlanId { get; set; }
    public int Type { get; set; }
    public string? Content { get; set; }
    public string? MediaUrl { get; set; }
    public DateTime CheckInDate { get; set; }
    public bool? FaceVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCheckInDto
{
    [Required]
    public long ChildId { get; set; }
    public long? PlanId { get; set; }

    /// <summary>1=文字 2=拍照 3=视频</summary>
    [Required]
    public int Type { get; set; }

    public string? Content { get; set; }

    /// <summary>Base64编码的图片/视频数据</summary>
    public string? MediaData { get; set; }

    /// <summary>人脸验证图片Base64</summary>
    public string? FaceImageData { get; set; }
}

public class CheckInStatsDto
{
    public long ChildId { get; set; }
    public int TotalDays { get; set; }
    public int CurrentStreak { get; set; }
    public int LongestStreak { get; set; }
    public int ThisWeekDays { get; set; }
    public bool CheckedInToday { get; set; }
    public List<CheckInCalendarDto> Calendar { get; set; } = new();
}

public class CheckInCalendarDto
{
    public DateTime Date { get; set; }
    public bool HasCheckIn { get; set; }
    public int Type { get; set; }
}

// ==================== 学习提醒 ====================

public class StudyReminderDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public int Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? ReminderTime { get; set; }
    public bool IsEnabled { get; set; }
}

public class CreateReminderDto
{
    [Required]
    public long ChildId { get; set; }

    [Required]
    public int Type { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string? Content { get; set; }

    /// <summary>提醒时间 HH:mm</summary>
    public string? ReminderTime { get; set; }
}

// ==================== 成就与激励 ====================

public class AchievementDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }
    public int Type { get; set; }
    public int? Threshold { get; set; }
}

public class ChildAchievementDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public long AchievementId { get; set; }
    public string AchievementName { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public DateTime AchievedAt { get; set; }
}

public class EncouragementDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? TriggerReason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class PointsSummaryDto
{
    public long ChildId { get; set; }
    public int TotalPoints { get; set; }
    public int ThisWeekPoints { get; set; }
    public int TodayPoints { get; set; }
    public List<PointsTransactionDto> RecentTransactions { get; set; } = new();
}

public class PointsTransactionDto
{
    public long Id { get; set; }
    public int Points { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime TransactionDate { get; set; }
}

// ==================== 教育数据同步 ====================

public class EducationSyncStatusDto
{
    public string ProviderName { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public int StagesCount { get; set; }
    public int TextbooksCount { get; set; }
}

public class TextbookDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? CoverImage { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public string GradeName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public List<ChapterDetailDto> Chapters { get; set; } = new();
}

public class ChapterDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<LessonDetailDto> Lessons { get; set; } = new();
}

public class LessonDetailDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? Content { get; set; }
    public int? DurationMinutes { get; set; }
}

// ==================== 学习路径推荐 ====================

public class LearningRecommendationDto
{
    public long ChildId { get; set; }
    public List<RecommendedLessonDto> TodayLessons { get; set; } = new();
    public List<RecommendedLessonDto> WeakSubjects { get; set; } = new();
    public string? EncouragementMessage { get; set; }
}

public class RecommendedLessonDto
{
    public long LessonId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Reason { get; set; } = string.Empty;
}
