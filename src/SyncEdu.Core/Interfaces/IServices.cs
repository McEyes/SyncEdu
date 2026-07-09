using SyncEdu.Shared.DTOs;
using SyncEdu.Shared.Models;

namespace SyncEdu.Core.Interfaces;

public interface IAuthService
{
    Task<ApiResult<LoginResultDto>> RegisterAsync(RegisterDto dto);
    Task<ApiResult<LoginResultDto>> LoginAsync(LoginDto dto);
}

public interface IFamilyService
{
    Task<ApiResult<FamilyDto>> CreateFamilyAsync(long userId, CreateFamilyDto dto);
    Task<ApiResult<FamilyDto>> GetFamilyAsync(long userId);
    Task<ApiResult> JoinFamilyAsync(long userId, string inviteCode);
}

public interface IChildService
{
    Task<ApiResult<ChildDto>> CreateChildAsync(long userId, CreateChildDto dto);
    Task<ApiResult<List<ChildDto>>> GetChildrenAsync(long userId);
    Task<ApiResult<ChildDto>> UpdateChildAsync(UpdateChildDto dto);
    Task<ApiResult> DeleteChildAsync(long childId);
    Task<ApiResult> UpdateChildProfileAsync(UpdateChildProfileDto dto);
}

public interface IEducationService
{
    Task<ApiResult<List<EducationStageDto>>> GetStagesAsync();
    Task<ApiResult<List<GradeDto>>> GetGradesByStageAsync(long stageId);
    Task<ApiResult<List<TextbookVersionDto>>> GetTextbookVersionsAsync();
    Task<ApiResult<List<SubjectDto>>> GetSubjectsAsync();
    Task<ApiResult<List<TextbookDto>>> GetTextbooksAsync(long gradeId, long subjectId, int? semester);
    Task<ApiResult<List<ChildSubjectConfigDto>>> GetChildSubjectConfigsAsync(long childId);
    Task<ApiResult> SetChildSubjectConfigAsync(SetChildSubjectConfigDto dto);
    Task<ApiResult> DeleteChildSubjectConfigAsync(long configId);
}

public interface IDashboardService
{
    Task<ApiResult<DashboardDto>> GetDashboardAsync(long userId);
}

// ==================== Phase 2 服务接口 ====================

public interface ILearningService
{
    Task<ApiResult<List<LearningPlanDto>>> GetPlansAsync(long childId);
    Task<ApiResult<LearningPlanDto>> CreatePlanAsync(CreateLearningPlanDto dto);
    Task<ApiResult<List<LearningProgressDto>>> GetProgressAsync(long planId, long childId);
    Task<ApiResult> UpdateProgressAsync(UpdateProgressDto dto);
    Task<ApiResult<StudySessionDto>> StartSessionAsync(StartSessionDto dto);
    Task<ApiResult> EndSessionAsync(long sessionId);
}

public interface ICheckInService
{
    Task<ApiResult<CheckInDto>> CreateCheckInAsync(CreateCheckInDto dto);
    Task<ApiResult<List<CheckInDto>>> GetCheckInsAsync(long childId, DateTime? startDate, DateTime? endDate);
    Task<ApiResult<CheckInStatsDto>> GetStatsAsync(long childId);
}

public interface IReminderService
{
    Task<ApiResult<List<StudyReminderDto>>> GetRemindersAsync(long childId);
    Task<ApiResult<StudyReminderDto>> CreateReminderAsync(CreateReminderDto dto);
    Task<ApiResult> ToggleReminderAsync(long reminderId, bool isEnabled);
    Task<ApiResult> DeleteReminderAsync(long reminderId);
}

public interface IAchievementService
{
    Task<ApiResult<List<AchievementDto>>> GetAllAchievementsAsync();
    Task<ApiResult<List<ChildAchievementDto>>> GetChildAchievementsAsync(long childId);
    Task<ApiResult<PointsSummaryDto>> GetPointsSummaryAsync(long childId);
    Task<ApiResult<List<EncouragementDto>>> GetEncouragementsAsync(long childId);
    Task<ApiResult> AddEncouragementAsync(long childId, string content, string? reason);
    Task<ApiResult> AwardPointsAsync(long childId, int points, string reason);
}

public interface IEducationSyncService
{
    Task<ApiResult<EducationSyncStatusDto>> GetSyncStatusAsync();
    Task<ApiResult> SyncEducationDataAsync();
    Task<ApiResult<TextbookDetailDto>> GetTextbookDetailAsync(long textbookId);
    Task<ApiResult<LearningRecommendationDto>> GetRecommendationsAsync(long childId);
}
