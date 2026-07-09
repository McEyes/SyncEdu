namespace SyncEdu.Shared.Enums;

/// <summary>
/// 教育阶段
/// </summary>
public enum EducationStage
{
    /// <summary>幼儿园</summary>
    Kindergarten = 1,
    /// <summary>小学</summary>
    Primary = 2,
    /// <summary>初中</summary>
    JuniorHigh = 3,
    /// <summary>高中</summary>
    SeniorHigh = 4,
    /// <summary>大学</summary>
    University = 5
}

/// <summary>
/// 打卡类型
/// </summary>
public enum CheckInType
{
    /// <summary>文字打卡</summary>
    Text = 1,
    /// <summary>拍照打卡</summary>
    Photo = 2,
    /// <summary>视频打卡</summary>
    Video = 3
}

/// <summary>
/// 提醒类型
/// </summary>
public enum ReminderType
{
    /// <summary>每日学习提醒</summary>
    DailyStudy = 1,
    /// <summary>打卡提醒</summary>
    CheckIn = 2,
    /// <summary>目标完成提醒</summary>
    GoalComplete = 3
}

/// <summary>
/// 成就类型
/// </summary>
public enum AchievementType
{
    /// <summary>连续打卡</summary>
    StreakCheckIn = 1,
    /// <summary>完成课程</summary>
    CourseComplete = 2,
    /// <summary>学习时长</summary>
    StudyDuration = 3,
    /// <summary>阶段达成</summary>
    StageComplete = 4
}
