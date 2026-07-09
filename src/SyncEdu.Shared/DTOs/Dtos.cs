using System.ComponentModel.DataAnnotations;

namespace SyncEdu.Shared.DTOs;

// ==================== 认证相关 ====================

public class RegisterDto
{
    [Required, Phone(ErrorMessage = "手机号格式不正确")]
    public string Phone { get; set; } = string.Empty;

    [Required, MinLength(6, ErrorMessage = "密码至少6位")]
    public string Password { get; set; } = string.Empty;

    [Required]
    public string NickName { get; set; } = string.Empty;

    public string? SmsCode { get; set; }
}

public class LoginDto
{
    [Required]
    public string Phone { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public long UserId { get; set; }
}

// ==================== 家庭相关 ====================

public class FamilyDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? InviteCode { get; set; }
    public DateTime CreatedAt { get; set; }
    public int MemberCount { get; set; }
    public int ChildCount { get; set; }
}

public class CreateFamilyDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

// ==================== 小孩相关 ====================

public class ChildDto
{
    public long Id { get; set; }
    public string NickName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public DateTime? Birthday { get; set; }
    public int Gender { get; set; }
    public long FamilyId { get; set; }
    public string? StageName { get; set; }
    public string? GradeName { get; set; }
    public string? SchoolName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateChildDto
{
    [Required]
    public string NickName { get; set; } = string.Empty;

    public string? Avatar { get; set; }
    public DateTime? Birthday { get; set; }

    /// <summary>1=男 2=女</summary>
    public int Gender { get; set; }
}

public class UpdateChildDto
{
    [Required]
    public long Id { get; set; }

    public string? NickName { get; set; }
    public string? Avatar { get; set; }
    public DateTime? Birthday { get; set; }
    public int? Gender { get; set; }
}

public class UpdateChildProfileDto
{
    [Required]
    public long ChildId { get; set; }

    public int? StageId { get; set; }
    public long? GradeId { get; set; }
    public string? SchoolName { get; set; }
    public long? TextbookVersionId { get; set; }
}

// ==================== 小孩科目教材配置 ====================

public class ChildSubjectConfigDto
{
    public long Id { get; set; }
    public long ChildId { get; set; }
    public long SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public long TextbookVersionId { get; set; }
    public string TextbookVersionName { get; set; } = string.Empty;
    public long GradeId { get; set; }
}

public class SetChildSubjectConfigDto
{
    [Required]
    public long ChildId { get; set; }
    [Required]
    public long SubjectId { get; set; }
    [Required]
    public long TextbookVersionId { get; set; }
    [Required]
    public long GradeId { get; set; }
}

// ==================== 教材查询 ====================

public class TextbookDto
{
    public long Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public long VersionId { get; set; }
    public string VersionName { get; set; } = string.Empty;
    public long GradeId { get; set; }
    public string GradeName { get; set; } = string.Empty;
    public long SubjectId { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public int Semester { get; set; }
    public string SemesterName => Semester == 1 ? "上册" : "下册";
}

// ==================== 教育体系 ====================

public class EducationStageDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public List<GradeDto> Grades { get; set; } = new();
}

public class GradeDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public long StageId { get; set; }
    public int SortOrder { get; set; }
}

public class TextbookVersionDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Publisher { get; set; }
}

public class SubjectDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int SortOrder { get; set; }
}

// ==================== 仪表盘 ====================

public class DashboardDto
{
    public List<ChildOverviewDto> Children { get; set; } = new();
}

public class ChildOverviewDto
{
    public long ChildId { get; set; }
    public string NickName { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public string? StageName { get; set; }
    public string? GradeName { get; set; }
    public int TodayStudyMinutes { get; set; }
    public int TotalCheckInDays { get; set; }
    public int CurrentStreak { get; set; }
    public double ProgressPercent { get; set; }
}
