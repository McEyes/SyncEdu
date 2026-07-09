using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncEdu.Core.Interfaces;
using SyncEdu.Shared.DTOs;
using System.Security.Claims;

namespace SyncEdu.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    /// <summary>注册</summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);
        return Ok(result);
    }

    /// <summary>登录</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FamilyController : ControllerBase
{
    private readonly IFamilyService _familyService;

    public FamilyController(IFamilyService familyService) => _familyService = familyService;

    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>创建家庭</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFamilyDto dto)
    {
        var result = await _familyService.CreateFamilyAsync(UserId, dto);
        return Ok(result);
    }

    /// <summary>获取我的家庭</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _familyService.GetFamilyAsync(UserId);
        return Ok(result);
    }

    /// <summary>通过邀请码加入家庭</summary>
    [HttpPost("join")]
    public async Task<IActionResult> Join([FromBody] string inviteCode)
    {
        var result = await _familyService.JoinFamilyAsync(UserId, inviteCode);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChildController : ControllerBase
{
    private readonly IChildService _childService;

    public ChildController(IChildService childService) => _childService = childService;

    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>添加小孩</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateChildDto dto)
    {
        var result = await _childService.CreateChildAsync(UserId, dto);
        return Ok(result);
    }

    /// <summary>获取家庭下所有小孩</summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var result = await _childService.GetChildrenAsync(UserId);
        return Ok(result);
    }

    /// <summary>更新小孩信息</summary>
    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateChildDto dto)
    {
        var result = await _childService.UpdateChildAsync(dto);
        return Ok(result);
    }

    /// <summary>删除小孩</summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _childService.DeleteChildAsync(id);
        return Ok(result);
    }

    /// <summary>更新小孩学习档案</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateChildProfileDto dto)
    {
        var result = await _childService.UpdateChildProfileAsync(dto);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
public class EducationController : ControllerBase
{
    private readonly IEducationService _educationService;

    public EducationController(IEducationService educationService) => _educationService = educationService;

    /// <summary>获取所有教育阶段及年级</summary>
    [HttpGet("stages")]
    public async Task<IActionResult> GetStages()
    {
        var result = await _educationService.GetStagesAsync();
        return Ok(result);
    }

    /// <summary>获取指定阶段的年级列表</summary>
    [HttpGet("grades/{stageId}")]
    public async Task<IActionResult> GetGrades(long stageId)
    {
        var result = await _educationService.GetGradesByStageAsync(stageId);
        return Ok(result);
    }

    /// <summary>获取教材版本列表</summary>
    [HttpGet("textbook-versions")]
    public async Task<IActionResult> GetTextbookVersions()
    {
        var result = await _educationService.GetTextbookVersionsAsync();
        return Ok(result);
    }

    /// <summary>获取所有科目列表</summary>
    [HttpGet("subjects")]
    public async Task<IActionResult> GetSubjects()
    {
        var result = await _educationService.GetSubjectsAsync();
        return Ok(result);
    }

    /// <summary>按年级+科目+学期查询教材</summary>
    [HttpGet("textbooks")]
    public async Task<IActionResult> GetTextbooks([FromQuery] long gradeId, [FromQuery] long subjectId, [FromQuery] int? semester)
    {
        var result = await _educationService.GetTextbooksAsync(gradeId, subjectId, semester);
        return Ok(result);
    }

    /// <summary>获取小孩科目教材配置</summary>
    [HttpGet("child-subject-config/{childId}")]
    public async Task<IActionResult> GetChildSubjectConfigs(long childId)
    {
        var result = await _educationService.GetChildSubjectConfigsAsync(childId);
        return Ok(result);
    }

    /// <summary>设置小孩科目教材配置</summary>
    [HttpPost("child-subject-config")]
    public async Task<IActionResult> SetChildSubjectConfig([FromBody] SetChildSubjectConfigDto dto)
    {
        var result = await _educationService.SetChildSubjectConfigAsync(dto);
        return Ok(result);
    }

    /// <summary>删除小孩科目教材配置</summary>
    [HttpDelete("child-subject-config/{configId}")]
    public async Task<IActionResult> DeleteChildSubjectConfig(long configId)
    {
        var result = await _educationService.DeleteChildSubjectConfigAsync(configId);
        return Ok(result);
    }
}

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService) => _dashboardService = dashboardService;

    private long UserId => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>获取仪表盘数据（各小孩学习概览）</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _dashboardService.GetDashboardAsync(UserId);
        return Ok(result);
    }
}
