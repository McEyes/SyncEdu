using SqlSugar;
using SyncEdu.Core.Entities;
using SyncEdu.Core.Interfaces;
using SyncEdu.Shared.DTOs;
using SyncEdu.Shared.Models;

namespace SyncEdu.Core.Services;

// ==================== 学习计划与进度服务 ====================

public class LearningService : ILearningService
{
    private readonly ISqlSugarClient _db;
    public LearningService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<List<LearningPlanDto>>> GetPlansAsync(long childId)
    {
        var plans = await _db.Queryable<LearningPlan>()
            .Where(p => p.ChildId == childId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var dtos = new List<LearningPlanDto>();
        foreach (var plan in plans)
        {
            var textbook = await _db.Queryable<Textbook>().FirstAsync(t => t.Id == plan.TextbookId);
            var child = await _db.Queryable<Child>().FirstAsync(c => c.Id == plan.ChildId);
            var totalLessons = await GetTotalLessons(plan.TextbookId);
            var completedLessons = await _db.Queryable<LearningProgress>()
                .CountAsync(p => p.PlanId == plan.Id && p.Status == 3 && !p.IsDeleted);

            dtos.Add(new LearningPlanDto
            {
                Id = plan.Id,
                ChildId = plan.ChildId,
                ChildName = child?.NickName ?? "",
                TextbookId = plan.TextbookId,
                TextbookTitle = textbook?.Title ?? "",
                Title = plan.Title,
                StartDate = plan.StartDate,
                EndDate = plan.EndDate,
                ProgressPercent = totalLessons > 0 ? completedLessons * 100 / totalLessons : 0,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                CreatedAt = plan.CreatedAt
            });
        }
        return ApiResult<List<LearningPlanDto>>.Success(dtos);
    }

    public async Task<ApiResult<LearningPlanDto>> CreatePlanAsync(CreateLearningPlanDto dto)
    {
        // 根据年级+科目+版本+学期查找教材
        var textbook = await _db.Queryable<Textbook>()
            .FirstAsync(t => t.GradeId == dto.GradeId && t.SubjectId == dto.SubjectId
                             && t.VersionId == dto.TextbookVersionId && t.Semester == dto.Semester
                             && !t.IsDeleted);

        if (textbook == null)
            return ApiResult<LearningPlanDto>.Fail("未找到对应教材，请先同步教育数据");

        var child = await _db.Queryable<Child>().FirstAsync(c => c.Id == dto.ChildId);
        var grade = await _db.Queryable<Grade>().FirstAsync(g => g.Id == dto.GradeId);
        var subject = await _db.Queryable<Subject>().FirstAsync(s => s.Id == dto.SubjectId);
        var semesterName = dto.Semester == 1 ? "上册" : "下册";

        // 自动生成标题
        var title = $"{grade?.Name ?? ""}{subject?.Name ?? ""}{semesterName}";

        // 计算日期
        var startDate = dto.StartDate ?? DateTime.Today;
        var endDate = dto.EndDate;
        if (dto.DurationDays.HasValue && dto.DurationDays.Value > 0)
        {
            endDate = startDate.AddDays(dto.DurationDays.Value - 1);
        }

        var plan = new LearningPlan
        {
            ChildId = dto.ChildId,
            TextbookId = textbook.Id,
            Title = title,
            StartDate = startDate,
            EndDate = endDate,
            ProgressPercent = 0,
            CreatedAt = DateTime.UtcNow
        };
        var id = await _db.Insertable(plan).ExecuteReturnIdentityAsync();
        plan.Id = id;

        // 自动创建所有课时的学习进度记录
        var chapters = await _db.Queryable<Chapter>()
            .Where(c => c.TextbookId == textbook.Id && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        foreach (var chapter in chapters)
        {
            var lessons = await _db.Queryable<Lesson>()
                .Where(l => l.ChapterId == chapter.Id && !l.IsDeleted)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            foreach (var lesson in lessons)
            {
                await _db.Insertable(new LearningProgress
                {
                    PlanId = plan.Id,
                    LessonId = lesson.Id,
                    ChildId = dto.ChildId,
                    Status = 1, // 未开始
                    CreatedAt = DateTime.UtcNow
                }).ExecuteReturnIdentityAsync();
            }
        }

        var totalLessons = await GetTotalLessons(textbook.Id);

        return ApiResult<LearningPlanDto>.Success(new LearningPlanDto
        {
            Id = plan.Id,
            ChildId = plan.ChildId,
            ChildName = child?.NickName ?? "",
            TextbookId = plan.TextbookId,
            TextbookTitle = textbook.Title,
            SubjectName = subject?.Name ?? "",
            GradeName = grade?.Name ?? "",
            Semester = dto.Semester,
            Title = plan.Title,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            ProgressPercent = 0,
            TotalLessons = totalLessons,
            CompletedLessons = 0,
            CreatedAt = plan.CreatedAt
        });
    }

    public async Task<ApiResult<List<LearningProgressDto>>> GetProgressAsync(long planId, long childId)
    {
        var progressList = await _db.Queryable<LearningProgress>()
            .Where(p => p.PlanId == planId && p.ChildId == childId && !p.IsDeleted)
            .ToListAsync();

        var dtos = new List<LearningProgressDto>();
        foreach (var p in progressList)
        {
            var lesson = await _db.Queryable<Lesson>().FirstAsync(l => l.Id == p.LessonId);
            string chapterTitle = "";
            if (lesson != null)
            {
                var chapter = await _db.Queryable<Chapter>().FirstAsync(c => c.Id == lesson.ChapterId);
                chapterTitle = chapter?.Title ?? "";
            }
            dtos.Add(new LearningProgressDto
            {
                Id = p.Id,
                PlanId = p.PlanId,
                LessonId = p.LessonId,
                LessonTitle = lesson?.Title ?? "",
                ChapterTitle = chapterTitle,
                Status = p.Status,
                CompletedAt = p.CompletedAt,
                StudyMinutes = p.StudyMinutes
            });
        }
        return ApiResult<List<LearningProgressDto>>.Success(dtos);
    }

    public async Task<ApiResult> UpdateProgressAsync(UpdateProgressDto dto)
    {
        var existing = await _db.Queryable<LearningProgress>()
            .FirstAsync(p => p.PlanId == dto.PlanId && p.LessonId == dto.LessonId && p.ChildId == dto.ChildId && !p.IsDeleted);

        if (existing == null)
        {
            existing = new LearningProgress
            {
                PlanId = dto.PlanId,
                LessonId = dto.LessonId,
                ChildId = dto.ChildId,
                Status = dto.Status,
                StudyMinutes = dto.StudyMinutes,
                CompletedAt = dto.Status == 3 ? DateTime.UtcNow : null,
                CreatedAt = DateTime.UtcNow
            };
            await _db.Insertable(existing).ExecuteReturnIdentityAsync();
        }
        else
        {
            existing.Status = dto.Status;
            existing.StudyMinutes = dto.StudyMinutes;
            if (dto.Status == 3) existing.CompletedAt = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.Updateable(existing).ExecuteCommandAsync();
        }

        // 更新计划进度
        var totalLessons = await _db.Queryable<LearningProgress>()
            .CountAsync(p => p.PlanId == dto.PlanId && p.ChildId == dto.ChildId && !p.IsDeleted);
        var completedLessons = await _db.Queryable<LearningProgress>()
            .CountAsync(p => p.PlanId == dto.PlanId && p.ChildId == dto.ChildId && p.Status == 3 && !p.IsDeleted);

        if (totalLessons > 0)
        {
            var percent = completedLessons * 100 / totalLessons;
            await _db.Updateable<LearningPlan>()
                .SetColumns(p => p.ProgressPercent == percent)
                .SetColumns(p => p.UpdatedAt == DateTime.UtcNow)
                .Where(p => p.Id == dto.PlanId)
                .ExecuteCommandAsync();
        }

        return ApiResult.Success("更新成功");
    }

    public async Task<ApiResult<StudySessionDto>> StartSessionAsync(StartSessionDto dto)
    {
        var session = new StudySession
        {
            ChildId = dto.ChildId,
            PlanId = dto.PlanId,
            LessonId = dto.LessonId,
            StartTime = DateTime.UtcNow,
            Status = 1,
            DurationMinutes = 0,
            CreatedAt = DateTime.UtcNow
        };
        var id = await _db.Insertable(session).ExecuteReturnIdentityAsync();
        session.Id = id;

        return ApiResult<StudySessionDto>.Success(new StudySessionDto
        {
            Id = id,
            ChildId = session.ChildId,
            PlanId = session.PlanId,
            LessonId = session.LessonId,
            StartTime = session.StartTime
        });
    }

    public async Task<ApiResult> EndSessionAsync(long sessionId)
    {
        var session = await _db.Queryable<StudySession>().FirstAsync(s => s.Id == sessionId);
        if (session == null)
            return ApiResult.Fail("会话不存在");

        session.EndTime = DateTime.UtcNow;
        session.Status = 2;
        session.DurationMinutes = (int)(session.EndTime.Value - session.StartTime).TotalMinutes;
        session.UpdatedAt = DateTime.UtcNow;
        await _db.Updateable(session).ExecuteCommandAsync();

        return ApiResult.Success("会话已结束");
    }

    private async Task<int> GetTotalLessons(long textbookId)
    {
        var chapters = await _db.Queryable<Chapter>()
            .Where(c => c.TextbookId == textbookId && !c.IsDeleted)
            .ToListAsync();
        int total = 0;
        foreach (var chapter in chapters)
        {
            total += await _db.Queryable<Lesson>()
                .CountAsync(l => l.ChapterId == chapter.Id && !l.IsDeleted);
        }
        return total;
    }
}

// ==================== 打卡服务 ====================

public class CheckInService : ICheckInService
{
    private readonly ISqlSugarClient _db;
    public CheckInService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<CheckInDto>> CreateCheckInAsync(CreateCheckInDto dto)
    {
        string? mediaUrl = null;
        bool? faceVerified = null;

        // 处理媒体数据（Base64 → 存储路径，简化为直接保存标记）
        if (!string.IsNullOrEmpty(dto.MediaData))
        {
            mediaUrl = $"media/{dto.ChildId}/{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
        }

        // 人脸验证（简化实现：有图片数据就标记为已验证）
        if (!string.IsNullOrEmpty(dto.FaceImageData))
        {
            faceVerified = true; // 实际应调用AI人脸比对API
        }

        var checkIn = new CheckIn
        {
            ChildId = dto.ChildId,
            PlanId = dto.PlanId,
            Type = dto.Type,
            Content = dto.Content,
            MediaUrl = mediaUrl,
            CheckInDate = DateTime.UtcNow,
            FaceVerified = faceVerified,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _db.Insertable(checkIn).ExecuteReturnIdentityAsync();
        checkIn.Id = id;

        var child = await _db.Queryable<Child>().FirstAsync(c => c.Id == dto.ChildId);

        // 打卡奖励积分
        await AwardCheckInPoints(dto.ChildId);

        return ApiResult<CheckInDto>.Success(new CheckInDto
        {
            Id = checkIn.Id,
            ChildId = checkIn.ChildId,
            ChildName = child?.NickName ?? "",
            PlanId = checkIn.PlanId,
            Type = checkIn.Type,
            Content = checkIn.Content,
            MediaUrl = checkIn.MediaUrl,
            CheckInDate = checkIn.CheckInDate,
            FaceVerified = checkIn.FaceVerified,
            CreatedAt = checkIn.CreatedAt
        });
    }

    public async Task<ApiResult<List<CheckInDto>>> GetCheckInsAsync(long childId, DateTime? startDate, DateTime? endDate)
    {
        var query = _db.Queryable<CheckIn>()
            .Where(c => c.ChildId == childId && !c.IsDeleted);

        if (startDate.HasValue)
            query = query.Where(c => c.CheckInDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(c => c.CheckInDate <= endDate.Value);

        var checkIns = await query.OrderByDescending(c => c.CheckInDate).ToListAsync();
        var child = await _db.Queryable<Child>().FirstAsync(c => c.Id == childId);

        var dtos = checkIns.Select(c => new CheckInDto
        {
            Id = c.Id,
            ChildId = c.ChildId,
            ChildName = child?.NickName ?? "",
            PlanId = c.PlanId,
            Type = c.Type,
            Content = c.Content,
            MediaUrl = c.MediaUrl,
            CheckInDate = c.CheckInDate,
            FaceVerified = c.FaceVerified,
            CreatedAt = c.CreatedAt
        }).ToList();

        return ApiResult<List<CheckInDto>>.Success(dtos);
    }

    public async Task<ApiResult<CheckInStatsDto>> GetStatsAsync(long childId)
    {
        var allCheckIns = await _db.Queryable<CheckIn>()
            .Where(c => c.ChildId == childId && !c.IsDeleted)
            .OrderByDescending(c => c.CheckInDate)
            .ToListAsync();

        var today = DateTime.UtcNow.Date;
        var uniqueDays = allCheckIns.Select(c => c.CheckInDate.Date).Distinct().ToList();

        // 计算连续打卡
        int currentStreak = 0;
        var checkDate = today;
        foreach (var _ in uniqueDays)
        {
            if (uniqueDays.Contains(checkDate))
            {
                currentStreak++;
                checkDate = checkDate.AddDays(-1);
            }
            else break;
        }

        // 最长连续
        int longestStreak = 0, tempStreak = 0;
        var sortedDays = uniqueDays.OrderBy(d => d).ToList();
        for (int i = 0; i < sortedDays.Count; i++)
        {
            if (i == 0) { tempStreak = 1; }
            else if (sortedDays[i] == sortedDays[i - 1].AddDays(1)) { tempStreak++; }
            else { tempStreak = 1; }
            longestStreak = Math.Max(longestStreak, tempStreak);
        }

        // 本周打卡
        var weekStart = today.AddDays(-(int)today.DayOfWeek);
        var thisWeekDays = uniqueDays.Count(d => d >= weekStart);

        // 打卡日历（最近30天）
        var calendar = new List<CheckInCalendarDto>();
        for (int i = 29; i >= 0; i--)
        {
            var date = today.AddDays(-i);
            calendar.Add(new CheckInCalendarDto
            {
                Date = date,
                HasCheckIn = uniqueDays.Contains(date),
                Type = allCheckIns.FirstOrDefault(c => c.CheckInDate.Date == date)?.Type ?? 0
            });
        }

        return ApiResult<CheckInStatsDto>.Success(new CheckInStatsDto
        {
            ChildId = childId,
            TotalDays = uniqueDays.Count,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            ThisWeekDays = thisWeekDays,
            CheckedInToday = uniqueDays.Contains(today),
            Calendar = calendar
        });
    }

    private async Task AwardCheckInPoints(long childId)
    {
        await _db.Insertable(new PointsTransaction
        {
            ChildId = childId,
            Points = 5,
            Reason = "每日打卡",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();
    }
}

// ==================== 学习提醒服务 ====================

public class ReminderService : IReminderService
{
    private readonly ISqlSugarClient _db;
    public ReminderService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<List<StudyReminderDto>>> GetRemindersAsync(long childId)
    {
        var reminders = await _db.Queryable<StudyReminder>()
            .Where(r => r.ChildId == childId && !r.IsDeleted)
            .OrderBy(r => r.ReminderTime)
            .ToListAsync();

        return ApiResult<List<StudyReminderDto>>.Success(reminders.Select(r => new StudyReminderDto
        {
            Id = r.Id,
            ChildId = r.ChildId,
            Type = r.Type,
            Title = r.Title,
            Content = r.Content,
            ReminderTime = r.ReminderTime?.ToString(@"hh\:mm"),
            IsEnabled = r.IsEnabled
        }).ToList());
    }

    public async Task<ApiResult<StudyReminderDto>> CreateReminderAsync(CreateReminderDto dto)
    {
        TimeSpan? time = null;
        if (!string.IsNullOrEmpty(dto.ReminderTime) && TimeSpan.TryParse(dto.ReminderTime, out var parsed))
            time = parsed;

        var reminder = new StudyReminder
        {
            ChildId = dto.ChildId,
            Type = dto.Type,
            Title = dto.Title,
            Content = dto.Content,
            ReminderTime = time,
            IsEnabled = true,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _db.Insertable(reminder).ExecuteReturnIdentityAsync();
        reminder.Id = id;

        return ApiResult<StudyReminderDto>.Success(new StudyReminderDto
        {
            Id = reminder.Id,
            ChildId = reminder.ChildId,
            Type = reminder.Type,
            Title = reminder.Title,
            Content = reminder.Content,
            ReminderTime = reminder.ReminderTime?.ToString(@"hh\:mm"),
            IsEnabled = reminder.IsEnabled
        });
    }

    public async Task<ApiResult> ToggleReminderAsync(long reminderId, bool isEnabled)
    {
        await _db.Updateable<StudyReminder>()
            .SetColumns(r => r.IsEnabled == isEnabled)
            .SetColumns(r => r.UpdatedAt == DateTime.UtcNow)
            .Where(r => r.Id == reminderId)
            .ExecuteCommandAsync();
        return ApiResult.Success("更新成功");
    }

    public async Task<ApiResult> DeleteReminderAsync(long reminderId)
    {
        await _db.Updateable<StudyReminder>()
            .SetColumns(r => r.IsDeleted == true)
            .SetColumns(r => r.UpdatedAt == DateTime.UtcNow)
            .Where(r => r.Id == reminderId)
            .ExecuteCommandAsync();
        return ApiResult.Success("删除成功");
    }
}

// ==================== 成就与激励服务 ====================

public class AchievementService : IAchievementService
{
    private readonly ISqlSugarClient _db;
    public AchievementService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<List<AchievementDto>>> GetAllAchievementsAsync()
    {
        var achievements = await _db.Queryable<Achievement>()
            .Where(a => !a.IsDeleted)
            .OrderBy(a => a.Type)
            .ToListAsync();

        return ApiResult<List<AchievementDto>>.Success(achievements.Select(a => new AchievementDto
        {
            Id = a.Id,
            Name = a.Name,
            Description = a.Description,
            Icon = a.Icon,
            Type = a.Type,
            Threshold = a.Threshold
        }).ToList());
    }

    public async Task<ApiResult<List<ChildAchievementDto>>> GetChildAchievementsAsync(long childId)
    {
        var childAchievements = await _db.Queryable<ChildAchievement>()
            .Where(ca => ca.ChildId == childId && !ca.IsDeleted)
            .OrderByDescending(ca => ca.AchievedAt)
            .ToListAsync();

        var dtos = new List<ChildAchievementDto>();
        foreach (var ca in childAchievements)
        {
            var achievement = await _db.Queryable<Achievement>().FirstAsync(a => a.Id == ca.AchievementId);
            dtos.Add(new ChildAchievementDto
            {
                Id = ca.Id,
                ChildId = ca.ChildId,
                AchievementId = ca.AchievementId,
                AchievementName = achievement?.Name ?? "",
                Icon = achievement?.Icon,
                AchievedAt = ca.AchievedAt
            });
        }
        return ApiResult<List<ChildAchievementDto>>.Success(dtos);
    }

    public async Task<ApiResult<PointsSummaryDto>> GetPointsSummaryAsync(long childId)
    {
        var allTransactions = await _db.Queryable<PointsTransaction>()
            .Where(p => p.ChildId == childId && !p.IsDeleted)
            .OrderByDescending(p => p.TransactionDate)
            .ToListAsync();

        var totalPoints = allTransactions.Sum(p => p.Points);
        var today = DateTime.UtcNow.Date;
        var weekStart = today.AddDays(-(int)today.DayOfWeek);

        return ApiResult<PointsSummaryDto>.Success(new PointsSummaryDto
        {
            ChildId = childId,
            TotalPoints = totalPoints,
            ThisWeekPoints = allTransactions.Where(p => p.TransactionDate >= weekStart).Sum(p => p.Points),
            TodayPoints = allTransactions.Where(p => p.TransactionDate.Date == today).Sum(p => p.Points),
            RecentTransactions = allTransactions.Take(10).Select(p => new PointsTransactionDto
            {
                Id = p.Id,
                Points = p.Points,
                Reason = p.Reason,
                TransactionDate = p.TransactionDate
            }).ToList()
        });
    }

    public async Task<ApiResult<List<EncouragementDto>>> GetEncouragementsAsync(long childId)
    {
        var logs = await _db.Queryable<EncouragementLog>()
            .Where(e => e.ChildId == childId && !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Take(20)
            .ToListAsync();

        return ApiResult<List<EncouragementDto>>.Success(logs.Select(e => new EncouragementDto
        {
            Id = e.Id,
            ChildId = e.ChildId,
            Content = e.Content,
            TriggerReason = e.TriggerReason,
            CreatedAt = e.CreatedAt
        }).ToList());
    }

    public async Task<ApiResult> AddEncouragementAsync(long childId, string content, string? reason)
    {
        await _db.Insertable(new EncouragementLog
        {
            ChildId = childId,
            Content = content,
            TriggerReason = reason,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();
        return ApiResult.Success("鼓励已发送");
    }

    public async Task<ApiResult> AwardPointsAsync(long childId, int points, string reason)
    {
        await _db.Insertable(new PointsTransaction
        {
            ChildId = childId,
            Points = points,
            Reason = reason,
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();
        return ApiResult.Success($"已奖励 {points} 积分");
    }
}

// ==================== 教育资源同步服务 ====================

public class EducationSyncService : IEducationSyncService
{
    private readonly ISqlSugarClient _db;
    private readonly IEnumerable<IEducationDataProvider> _providers;

    public EducationSyncService(ISqlSugarClient db, IEnumerable<IEducationDataProvider> providers)
    {
        _db = db;
        _providers = providers;
    }

    public async Task<ApiResult<EducationSyncStatusDto>> GetSyncStatusAsync()
    {
        var provider = _providers.FirstOrDefault();
        var stagesCount = await _db.Queryable<EducationStage>().CountAsync(s => !s.IsDeleted);
        var textbooksCount = await _db.Queryable<Textbook>().CountAsync(t => !t.IsDeleted);

        return ApiResult<EducationSyncStatusDto>.Success(new EducationSyncStatusDto
        {
            ProviderName = provider?.ProviderName ?? "未配置",
            IsAvailable = provider != null && await provider.IsAvailableAsync(),
            LastSyncAt = null,
            StagesCount = stagesCount,
            TextbooksCount = textbooksCount
        });
    }

    public async Task<ApiResult> SyncEducationDataAsync()
    {
        foreach (var provider in _providers)
        {
            if (!await provider.IsAvailableAsync()) continue;

            var stages = await provider.SyncStagesAsync();
            foreach (var stageDto in stages)
            {
                var exists = await _db.Queryable<EducationStage>()
                    .AnyAsync(s => s.Name == stageDto.Name && !s.IsDeleted);
                if (!exists)
                {
                    await _db.Insertable(new EducationStage
                    {
                        Name = stageDto.Name,
                        SortOrder = stageDto.SortOrder,
                        CreatedAt = DateTime.UtcNow
                    }).ExecuteReturnIdentityAsync();
                }
            }
        }
        return ApiResult.Success("同步完成");
    }

    public async Task<ApiResult<TextbookDetailDto>> GetTextbookDetailAsync(long textbookId)
    {
        var textbook = await _db.Queryable<Textbook>().FirstAsync(t => t.Id == textbookId);
        if (textbook == null)
            return ApiResult<TextbookDetailDto>.Fail("教材不存在");

        var version = await _db.Queryable<TextbookVersion>().FirstAsync(v => v.Id == textbook.VersionId);
        var grade = await _db.Queryable<Grade>().FirstAsync(g => g.Id == textbook.GradeId);
        var subject = await _db.Queryable<Subject>().FirstAsync(s => s.Id == textbook.SubjectId);

        var chapters = await _db.Queryable<Chapter>()
            .Where(c => c.TextbookId == textbookId && !c.IsDeleted)
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var chapterDtos = new List<ChapterDetailDto>();
        foreach (var chapter in chapters)
        {
            var lessons = await _db.Queryable<Lesson>()
                .Where(l => l.ChapterId == chapter.Id && !l.IsDeleted)
                .OrderBy(l => l.SortOrder)
                .ToListAsync();

            chapterDtos.Add(new ChapterDetailDto
            {
                Id = chapter.Id,
                Title = chapter.Title,
                SortOrder = chapter.SortOrder,
                Lessons = lessons.Select(l => new LessonDetailDto
                {
                    Id = l.Id,
                    Title = l.Title,
                    SortOrder = l.SortOrder,
                    Content = l.Content,
                    DurationMinutes = l.DurationMinutes
                }).ToList()
            });
        }

        return ApiResult<TextbookDetailDto>.Success(new TextbookDetailDto
        {
            Id = textbook.Id,
            Title = textbook.Title,
            CoverImage = textbook.CoverImage,
            VersionName = version?.Name ?? "",
            GradeName = grade?.Name ?? "",
            SubjectName = subject?.Name ?? "",
            Chapters = chapterDtos
        });
    }

    public async Task<ApiResult<LearningRecommendationDto>> GetRecommendationsAsync(long childId)
    {
        // 简化推荐逻辑：基于学习进度推荐未完成的课时
        var profile = await _db.Queryable<ChildProfile>()
            .FirstAsync(p => p.ChildId == childId && !p.IsDeleted);

        var todayLessons = new List<RecommendedLessonDto>();
        var weakSubjects = new List<RecommendedLessonDto>();

        if (profile?.GradeId != null)
        {
            var textbooks = await _db.Queryable<Textbook>()
                .Where(t => t.GradeId == profile.GradeId && !t.IsDeleted)
                .Take(5)
                .ToListAsync();

            foreach (var textbook in textbooks)
            {
                var subject = await _db.Queryable<Subject>().FirstAsync(s => s.Id == textbook.SubjectId);
                var firstChapter = await _db.Queryable<Chapter>()
                    .Where(c => c.TextbookId == textbook.Id && !c.IsDeleted)
                    .OrderBy(c => c.SortOrder)
                    .FirstAsync();

                if (firstChapter != null)
                {
                    var firstLesson = await _db.Queryable<Lesson>()
                        .Where(l => l.ChapterId == firstChapter.Id && !l.IsDeleted)
                        .OrderBy(l => l.SortOrder)
                        .FirstAsync();

                    if (firstLesson != null)
                    {
                        todayLessons.Add(new RecommendedLessonDto
                        {
                            LessonId = firstLesson.Id,
                            Title = firstLesson.Title,
                            SubjectName = subject?.Name ?? "",
                            Priority = 1,
                            Reason = "继续学习"
                        });
                    }
                }
            }
        }

        // 生成鼓励语
        var encouragementMessages = new[]
        {
            "今天也要加油哦！你已经很棒了！",
            "坚持就是胜利，继续保持！",
            "每一步都是进步，加油！",
            "学习让你更强大，继续努力！",
            "你今天的学习会让未来更美好！"
        };
        var random = new Random();
        var message = encouragementMessages[random.Next(encouragementMessages.Length)];

        return ApiResult<LearningRecommendationDto>.Success(new LearningRecommendationDto
        {
            ChildId = childId,
            TodayLessons = todayLessons,
            WeakSubjects = weakSubjects,
            EncouragementMessage = message
        });
    }
}
