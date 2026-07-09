using SqlSugar;
using SyncEdu.Core.Entities;
using SyncEdu.Core.Interfaces;
using SyncEdu.Shared.DTOs;
using SyncEdu.Shared.Models;

namespace SyncEdu.Core.Services;

public class FamilyService : IFamilyService
{
    private readonly ISqlSugarClient _db;

    public FamilyService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<FamilyDto>> CreateFamilyAsync(long userId, CreateFamilyDto dto)
    {
        var family = new Family
        {
            Name = dto.Name,
            InviteCode = Guid.NewGuid().ToString("N")[..8].ToUpper(),
            CreatedAt = DateTime.UtcNow
        };

        var familyId = await _db.Insertable(family).ExecuteReturnIdentityAsync();
        family.Id = familyId;

        await _db.Insertable(new FamilyMember
        {
            FamilyId = familyId,
            UserId = userId,
            Role = 1,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();

        return ApiResult<FamilyDto>.Success(MapToDto(family, 1, 0));
    }

    public async Task<ApiResult<FamilyDto>> GetFamilyAsync(long userId)
    {
        var member = await _db.Queryable<FamilyMember>()
            .FirstAsync(m => m.UserId == userId && !m.IsDeleted);

        if (member == null)
            return ApiResult<FamilyDto>.Fail("未加入任何家庭");

        var family = await _db.Queryable<Family>()
            .FirstAsync(f => f.Id == member.FamilyId && !f.IsDeleted);

        if (family == null)
            return ApiResult<FamilyDto>.Fail("家庭不存在");

        var memberCount = await _db.Queryable<FamilyMember>()
            .CountAsync(m => m.FamilyId == family.Id && !m.IsDeleted);

        var childCount = await _db.Queryable<Child>()
            .CountAsync(c => c.FamilyId == family.Id && !c.IsDeleted);

        return ApiResult<FamilyDto>.Success(MapToDto(family, memberCount, childCount));
    }

    public async Task<ApiResult> JoinFamilyAsync(long userId, string inviteCode)
    {
        var family = await _db.Queryable<Family>()
            .FirstAsync(f => f.InviteCode == inviteCode && !f.IsDeleted);

        if (family == null)
            return ApiResult.Fail("邀请码无效");

        var exists = await _db.Queryable<FamilyMember>()
            .AnyAsync(m => m.FamilyId == family.Id && m.UserId == userId && !m.IsDeleted);

        if (exists)
            return ApiResult.Fail("已在该家庭中");

        await _db.Insertable(new FamilyMember
        {
            FamilyId = family.Id,
            UserId = userId,
            Role = 2,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();

        return ApiResult.Success("加入成功");
    }

    private static FamilyDto MapToDto(Family family, int memberCount, int childCount) => new()
    {
        Id = family.Id,
        Name = family.Name,
        InviteCode = family.InviteCode,
        CreatedAt = family.CreatedAt,
        MemberCount = memberCount,
        ChildCount = childCount
    };
}

public class ChildService : IChildService
{
    private readonly ISqlSugarClient _db;

    public ChildService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<ChildDto>> CreateChildAsync(long userId, CreateChildDto dto)
    {
        var member = await _db.Queryable<FamilyMember>()
            .FirstAsync(m => m.UserId == userId && !m.IsDeleted);

        if (member == null)
            return ApiResult<ChildDto>.Fail("请先创建或加入家庭");

        var child = new Child
        {
            FamilyId = member.FamilyId,
            NickName = dto.NickName,
            Avatar = dto.Avatar,
            Birthday = dto.Birthday,
            Gender = dto.Gender,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _db.Insertable(child).ExecuteReturnIdentityAsync();
        child.Id = id;

        // 创建默认学习档案
        await _db.Insertable(new ChildProfile
        {
            ChildId = id,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();

        return ApiResult<ChildDto>.Success(await MapToDto(child));
    }

    public async Task<ApiResult<List<ChildDto>>> GetChildrenAsync(long userId)
    {
        var member = await _db.Queryable<FamilyMember>()
            .FirstAsync(m => m.UserId == userId && !m.IsDeleted);

        if (member == null)
            return ApiResult<List<ChildDto>>.Success(new List<ChildDto>());

        var children = await _db.Queryable<Child>()
            .Where(c => c.FamilyId == member.FamilyId && !c.IsDeleted)
            .ToListAsync();

        var dtos = new List<ChildDto>();
        foreach (var child in children)
        {
            dtos.Add(await MapToDto(child));
        }

        return ApiResult<List<ChildDto>>.Success(dtos);
    }

    public async Task<ApiResult<ChildDto>> UpdateChildAsync(UpdateChildDto dto)
    {
        var child = await _db.Queryable<Child>().FirstAsync(c => c.Id == dto.Id && !c.IsDeleted);
        if (child == null)
            return ApiResult<ChildDto>.Fail("小孩不存在");

        if (dto.NickName != null) child.NickName = dto.NickName;
        if (dto.Avatar != null) child.Avatar = dto.Avatar;
        if (dto.Birthday.HasValue) child.Birthday = dto.Birthday;
        if (dto.Gender.HasValue) child.Gender = dto.Gender;
        child.UpdatedAt = DateTime.UtcNow;

        await _db.Updateable(child).ExecuteCommandAsync();
        return ApiResult<ChildDto>.Success(await MapToDto(child));
    }

    public async Task<ApiResult> DeleteChildAsync(long childId)
    {
        await _db.Updateable<Child>()
            .SetColumns(c => c.IsDeleted == true)
            .SetColumns(c => c.UpdatedAt == DateTime.UtcNow)
            .Where(c => c.Id == childId)
            .ExecuteCommandAsync();

        return ApiResult.Success("删除成功");
    }

    public async Task<ApiResult> UpdateChildProfileAsync(UpdateChildProfileDto dto)
    {
        var profile = await _db.Queryable<ChildProfile>()
            .FirstAsync(p => p.ChildId == dto.ChildId && !p.IsDeleted);

        if (profile == null)
            return ApiResult.Fail("学习档案不存在");

        if (dto.StageId.HasValue) profile.StageId = dto.StageId;
        if (dto.GradeId.HasValue) profile.GradeId = dto.GradeId;
        if (dto.SchoolName != null) profile.SchoolName = dto.SchoolName;
        if (dto.TextbookVersionId.HasValue) profile.TextbookVersionId = dto.TextbookVersionId;
        profile.UpdatedAt = DateTime.UtcNow;

        await _db.Updateable(profile).ExecuteCommandAsync();
        return ApiResult.Success("更新成功");
    }

    private async Task<ChildDto> MapToDto(Child child)
    {
        var profile = await _db.Queryable<ChildProfile>()
            .FirstAsync(p => p.ChildId == child.Id && !p.IsDeleted);

        string? stageName = null, gradeName = null;
        if (profile?.StageId != null)
        {
            var stage = await _db.Queryable<EducationStage>()
                .FirstAsync(s => s.Id == profile.StageId);
            stageName = stage?.Name;
        }
        if (profile?.GradeId != null)
        {
            var grade = await _db.Queryable<Grade>()
                .FirstAsync(g => g.Id == profile.GradeId);
            gradeName = grade?.Name;
        }

        return new ChildDto
        {
            Id = child.Id,
            NickName = child.NickName,
            Avatar = child.Avatar,
            Birthday = child.Birthday,
            Gender = child.Gender ?? 0,
            FamilyId = child.FamilyId,
            StageName = stageName,
            GradeName = gradeName,
            SchoolName = profile?.SchoolName,
            CreatedAt = child.CreatedAt
        };
    }
}

public class EducationService : IEducationService
{
    private readonly ISqlSugarClient _db;

    public EducationService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<List<EducationStageDto>>> GetStagesAsync()
    {
        var stages = await _db.Queryable<EducationStage>()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        var grades = await _db.Queryable<Grade>()
            .Where(g => !g.IsDeleted)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();

        var result = stages.Select(s => new EducationStageDto
        {
            Id = s.Id,
            Name = s.Name,
            SortOrder = s.SortOrder,
            Grades = grades.Where(g => g.StageId == s.Id).Select(g => new GradeDto
            {
                Id = g.Id,
                Name = g.Name,
                StageId = g.StageId,
                SortOrder = g.SortOrder
            }).ToList()
        }).ToList();

        return ApiResult<List<EducationStageDto>>.Success(result);
    }

    public async Task<ApiResult<List<GradeDto>>> GetGradesByStageAsync(long stageId)
    {
        var grades = await _db.Queryable<Grade>()
            .Where(g => g.StageId == stageId && !g.IsDeleted)
            .OrderBy(g => g.SortOrder)
            .ToListAsync();

        return ApiResult<List<GradeDto>>.Success(grades.Select(g => new GradeDto
        {
            Id = g.Id,
            Name = g.Name,
            StageId = g.StageId,
            SortOrder = g.SortOrder
        }).ToList());
    }

    public async Task<ApiResult<List<TextbookVersionDto>>> GetTextbookVersionsAsync()
    {
        var versions = await _db.Queryable<TextbookVersion>()
            .Where(v => !v.IsDeleted)
            .ToListAsync();

        return ApiResult<List<TextbookVersionDto>>.Success(versions.Select(v => new TextbookVersionDto
        {
            Id = v.Id,
            Name = v.Name,
            Publisher = v.Publisher
        }).ToList());
    }

    public async Task<ApiResult<List<SubjectDto>>> GetSubjectsAsync()
    {
        var subjects = await _db.Queryable<Subject>()
            .Where(s => !s.IsDeleted)
            .OrderBy(s => s.SortOrder)
            .ToListAsync();

        return ApiResult<List<SubjectDto>>.Success(subjects.Select(s => new SubjectDto
        {
            Id = s.Id,
            Name = s.Name,
            Icon = s.Icon,
            SortOrder = s.SortOrder
        }).ToList());
    }

    public async Task<ApiResult<List<TextbookDto>>> GetTextbooksAsync(long gradeId, long subjectId, int? semester)
    {
        var query = _db.Queryable<Textbook>()
            .Where(t => t.GradeId == gradeId && t.SubjectId == subjectId && !t.IsDeleted);
        if (semester.HasValue)
            query = query.Where(t => t.Semester == semester.Value);

        var textbooks = await query.OrderBy(t => t.Semester).ToListAsync();
        var versionIds = textbooks.Select(t => t.VersionId).Distinct().ToList();
        var versions = await _db.Queryable<TextbookVersion>().Where(v => versionIds.Contains(v.Id)).ToListAsync();
        var grade = await _db.Queryable<Grade>().FirstAsync(g => g.Id == gradeId);
        var subject = await _db.Queryable<Subject>().FirstAsync(s => s.Id == subjectId);

        return ApiResult<List<TextbookDto>>.Success(textbooks.Select(t => new TextbookDto
        {
            Id = t.Id,
            Title = t.Title,
            VersionId = t.VersionId,
            VersionName = versions.FirstOrDefault(v => v.Id == t.VersionId)?.Name ?? "",
            GradeId = t.GradeId,
            GradeName = grade?.Name ?? "",
            SubjectId = t.SubjectId,
            SubjectName = subject?.Name ?? "",
            Semester = t.Semester
        }).ToList());
    }

    public async Task<ApiResult<List<ChildSubjectConfigDto>>> GetChildSubjectConfigsAsync(long childId)
    {
        var configs = await _db.Queryable<ChildSubjectConfig>()
            .Where(c => c.ChildId == childId && !c.IsDeleted)
            .ToListAsync();

        var subjectIds = configs.Select(c => c.SubjectId).Distinct().ToList();
        var versionIds = configs.Select(c => c.TextbookVersionId).Distinct().ToList();
        var subjects = await _db.Queryable<Subject>().Where(s => subjectIds.Contains(s.Id)).ToListAsync();
        var versions = await _db.Queryable<TextbookVersion>().Where(v => versionIds.Contains(v.Id)).ToListAsync();

        return ApiResult<List<ChildSubjectConfigDto>>.Success(configs.Select(c => new ChildSubjectConfigDto
        {
            Id = c.Id,
            ChildId = c.ChildId,
            SubjectId = c.SubjectId,
            SubjectName = subjects.FirstOrDefault(s => s.Id == c.SubjectId)?.Name ?? "",
            TextbookVersionId = c.TextbookVersionId,
            TextbookVersionName = versions.FirstOrDefault(v => v.Id == c.TextbookVersionId)?.Name ?? "",
            GradeId = c.GradeId
        }).ToList());
    }

    public async Task<ApiResult> SetChildSubjectConfigAsync(SetChildSubjectConfigDto dto)
    {
        var existing = await _db.Queryable<ChildSubjectConfig>()
            .FirstAsync(c => c.ChildId == dto.ChildId && c.SubjectId == dto.SubjectId && !c.IsDeleted);

        if (existing != null)
        {
            existing.TextbookVersionId = dto.TextbookVersionId;
            existing.GradeId = dto.GradeId;
            existing.UpdatedAt = DateTime.UtcNow;
            await _db.Updateable(existing).ExecuteCommandAsync();
        }
        else
        {
            await _db.Insertable(new ChildSubjectConfig
            {
                ChildId = dto.ChildId,
                SubjectId = dto.SubjectId,
                TextbookVersionId = dto.TextbookVersionId,
                GradeId = dto.GradeId,
                CreatedAt = DateTime.UtcNow
            }).ExecuteReturnIdentityAsync();
        }
        return ApiResult.Success("设置成功");
    }

    public async Task<ApiResult> DeleteChildSubjectConfigAsync(long configId)
    {
        await _db.Updateable<ChildSubjectConfig>()
            .SetColumns(c => c.IsDeleted == true)
            .SetColumns(c => c.UpdatedAt == DateTime.UtcNow)
            .Where(c => c.Id == configId)
            .ExecuteCommandAsync();
        return ApiResult.Success("删除成功");
    }
}

public class DashboardService : IDashboardService
{
    private readonly ISqlSugarClient _db;

    public DashboardService(ISqlSugarClient db) => _db = db;

    public async Task<ApiResult<DashboardDto>> GetDashboardAsync(long userId)
    {
        var member = await _db.Queryable<FamilyMember>()
            .FirstAsync(m => m.UserId == userId && !m.IsDeleted);

        if (member == null)
            return ApiResult<DashboardDto>.Success(new DashboardDto());

        var children = await _db.Queryable<Child>()
            .Where(c => c.FamilyId == member.FamilyId && !c.IsDeleted)
            .ToListAsync();

        var overviews = new List<ChildOverviewDto>();
        foreach (var child in children)
        {
            var profile = await _db.Queryable<ChildProfile>()
                .FirstAsync(p => p.ChildId == child.Id && !p.IsDeleted);

            string? stageName = null, gradeName = null;
            if (profile?.StageId != null)
            {
                var stage = await _db.Queryable<EducationStage>().FirstAsync(s => s.Id == profile.StageId);
                stageName = stage?.Name;
            }
            if (profile?.GradeId != null)
            {
                var grade = await _db.Queryable<Grade>().FirstAsync(g => g.Id == profile.GradeId);
                gradeName = grade?.Name;
            }

            var totalCheckIns = await _db.Queryable<CheckIn>()
                .CountAsync(c => c.ChildId == child.Id && !c.IsDeleted);

            var today = DateTime.UtcNow.Date;
            var todayCheckIn = await _db.Queryable<CheckIn>()
                .AnyAsync(c => c.ChildId == child.Id && c.CheckInDate.Date == today && !c.IsDeleted);

            overviews.Add(new ChildOverviewDto
            {
                ChildId = child.Id,
                NickName = child.NickName,
                Avatar = child.Avatar,
                StageName = stageName,
                GradeName = gradeName,
                TotalCheckInDays = totalCheckIns,
                CurrentStreak = 0, // TODO: 计算连续打卡天数
                TodayStudyMinutes = 0, // TODO: 计算今日学习时长
                ProgressPercent = 0 // TODO: 计算学习进度
            });
        }

        return ApiResult<DashboardDto>.Success(new DashboardDto { Children = overviews });
    }
}
