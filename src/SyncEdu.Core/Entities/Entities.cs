using SqlSugar;
using SyncEdu.Shared.Models;

namespace SyncEdu.Core.Entities;

/// <summary>用户（家长）</summary>
[SugarTable("user")]
public class User : EntityBase
{
    [SugarColumn(Length = 20)]
    public string Phone { get; set; } = string.Empty;

    [SugarColumn(Length = 256)]
    public string PasswordHash { get; set; } = string.Empty;

    [SugarColumn(Length = 50)]
    public string NickName { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Avatar { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? LastLoginAt { get; set; }
}

/// <summary>家庭</summary>
[SugarTable("family")]
public class Family : EntityBase
{
    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? InviteCode { get; set; }
}

/// <summary>家庭成员</summary>
[SugarTable("family_member")]
public class FamilyMember : EntityBase
{
    public long FamilyId { get; set; }
    public long UserId { get; set; }

    /// <summary>1=管理员 2=成员</summary>
    [SugarColumn(DefaultValue = "2")]
    public int Role { get; set; }
}

/// <summary>小孩</summary>
[SugarTable("child")]
public class Child : EntityBase
{
    public long FamilyId { get; set; }

    [SugarColumn(Length = 50)]
    public string NickName { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Avatar { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? Birthday { get; set; }

    /// <summary>1=男 2=女</summary>
    [SugarColumn(IsNullable = true)]
    public int? Gender { get; set; }
}

/// <summary>小孩学习档案</summary>
[SugarTable("child_profile")]
public class ChildProfile : EntityBase
{
    public long ChildId { get; set; }
    public long? StageId { get; set; }
    public long? GradeId { get; set; }

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? SchoolName { get; set; }

    /// <summary>默认教材版本（已废弃，使用ChildSubjectConfig）</summary>
    [SugarColumn(IsNullable = true)]
    public long? TextbookVersionId { get; set; }
}

/// <summary>教育阶段</summary>
[SugarTable("education_stage")]
public class EducationStage : EntityBase
{
    [SugarColumn(Length = 50)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

/// <summary>年级</summary>
[SugarTable("grade")]
public class Grade : EntityBase
{
    public long StageId { get; set; }

    [SugarColumn(Length = 50)]
    public string Name { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

/// <summary>学科</summary>
[SugarTable("subject")]
public class Subject : EntityBase
{
    [SugarColumn(Length = 50)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 20, IsNullable = true)]
    public string? Icon { get; set; }

    public int SortOrder { get; set; }
}

/// <summary>教材版本</summary>
[SugarTable("textbook_version")]
public class TextbookVersion : EntityBase
{
    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 100, IsNullable = true)]
    public string? Publisher { get; set; }
}

/// <summary>小孩科目教材配置（每科可用不同教材版本）</summary>
[SugarTable("child_subject_config")]
public class ChildSubjectConfig : EntityBase
{
    public long ChildId { get; set; }
    public long SubjectId { get; set; }
    public long TextbookVersionId { get; set; }
    public long GradeId { get; set; }
}

/// <summary>教材</summary>
[SugarTable("textbook")]
public class Textbook : EntityBase
{
    public long VersionId { get; set; }
    public long GradeId { get; set; }
    public long SubjectId { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? CoverImage { get; set; }

    /// <summary>学期 1=上册 2=下册</summary>
    [SugarColumn(DefaultValue = "1")]
    public int Semester { get; set; }
}

/// <summary>章节</summary>
[SugarTable("chapter")]
public class Chapter : EntityBase
{
    public long TextbookId { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    [SugarColumn(IsNullable = true)]
    public long? ParentId { get; set; }
}

/// <summary>课时</summary>
[SugarTable("lesson")]
public class Lesson : EntityBase
{
    public long ChapterId { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public string? Content { get; set; }

    public int SortOrder { get; set; }

    /// <summary>预计学习时长(分钟)</summary>
    [SugarColumn(IsNullable = true)]
    public int? DurationMinutes { get; set; }
}

/// <summary>学习计划</summary>
[SugarTable("learning_plan")]
public class LearningPlan : EntityBase
{
    public long ChildId { get; set; }
    public long TextbookId { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(IsNullable = true)]
    public DateTime? StartDate { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? EndDate { get; set; }

    /// <summary>进度百分比 0-100</summary>
    [SugarColumn(DefaultValue = "0")]
    public int ProgressPercent { get; set; }
}

/// <summary>学习进度</summary>
[SugarTable("learning_progress")]
public class LearningProgress : EntityBase
{
    public long PlanId { get; set; }
    public long LessonId { get; set; }
    public long ChildId { get; set; }

    /// <summary>学习状态 1=未开始 2=学习中 3=已完成</summary>
    [SugarColumn(DefaultValue = "1")]
    public int Status { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? CompletedAt { get; set; }

    /// <summary>学习时长(分钟)</summary>
    [SugarColumn(IsNullable = true)]
    public int? StudyMinutes { get; set; }
}

/// <summary>学习会话</summary>
[SugarTable("study_session")]
public class StudySession : EntityBase
{
    public long ChildId { get; set; }
    public long? PlanId { get; set; }
    public long? LessonId { get; set; }

    public DateTime StartTime { get; set; }

    [SugarColumn(IsNullable = true)]
    public DateTime? EndTime { get; set; }

    /// <summary>学习时长(分钟)</summary>
    [SugarColumn(DefaultValue = "0")]
    public int DurationMinutes { get; set; }

    /// <summary>会话状态 1=进行中 2=已完成 3=已取消</summary>
    [SugarColumn(DefaultValue = "1")]
    public int Status { get; set; }
}

/// <summary>打卡记录</summary>
[SugarTable("check_in")]
public class CheckIn : EntityBase
{
    public long ChildId { get; set; }
    public long? PlanId { get; set; }

    /// <summary>打卡类型 1=文字 2=拍照 3=视频</summary>
    public int Type { get; set; }

    [SugarColumn(ColumnDataType = "text", IsNullable = true)]
    public string? Content { get; set; }

    [SugarColumn(Length = 1000, IsNullable = true)]
    public string? MediaUrl { get; set; }

    public DateTime CheckInDate { get; set; }

    /// <summary>是否通过人脸验证</summary>
    [SugarColumn(IsNullable = true)]
    public bool? FaceVerified { get; set; }
}

/// <summary>学习提醒</summary>
[SugarTable("study_reminder")]
public class StudyReminder : EntityBase
{
    public long ChildId { get; set; }

    /// <summary>提醒类型 1=每日学习 2=打卡 3=目标完成</summary>
    public int Type { get; set; }

    [SugarColumn(Length = 200)]
    public string Title { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Content { get; set; }

    /// <summary>提醒时间</summary>
    [SugarColumn(IsNullable = true)]
    public TimeSpan? ReminderTime { get; set; }

    /// <summary>是否启用</summary>
    [SugarColumn(DefaultValue = "true")]
    public bool IsEnabled { get; set; } = true;
}

/// <summary>成就定义</summary>
[SugarTable("achievement")]
public class Achievement : EntityBase
{
    [SugarColumn(Length = 100)]
    public string Name { get; set; } = string.Empty;

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Description { get; set; }

    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Icon { get; set; }

    /// <summary>成就类型 1=连续打卡 2=完成课程 3=学习时长 4=阶段达成</summary>
    public int Type { get; set; }

    /// <summary>达成条件值</summary>
    [SugarColumn(IsNullable = true)]
    public int? Threshold { get; set; }
}

/// <summary>小孩成就</summary>
[SugarTable("child_achievement")]
public class ChildAchievement : EntityBase
{
    public long ChildId { get; set; }
    public long AchievementId { get; set; }
    public DateTime AchievedAt { get; set; }
}

/// <summary>鼓励记录</summary>
[SugarTable("encouragement_log")]
public class EncouragementLog : EntityBase
{
    public long ChildId { get; set; }

    [SugarColumn(Length = 500)]
    public string Content { get; set; } = string.Empty;

    /// <summary>触发原因</summary>
    [SugarColumn(Length = 100, IsNullable = true)]
    public string? TriggerReason { get; set; }
}

/// <summary>积分流水</summary>
[SugarTable("points_transaction")]
public class PointsTransaction : EntityBase
{
    public long ChildId { get; set; }
    public int Points { get; set; }

    [SugarColumn(Length = 100)]
    public string Reason { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }
}
