using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SqlSugar;
using SyncEdu.Core.Entities;
using SyncEdu.Core.Interfaces;
using SyncEdu.Shared.DTOs;
using SyncEdu.Shared.Models;

namespace SyncEdu.Core.Services;

public class AuthService : IAuthService
{
    private readonly ISqlSugarClient _db;
    private readonly IConfiguration _config;

    public AuthService(ISqlSugarClient db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<ApiResult<LoginResultDto>> RegisterAsync(RegisterDto dto)
    {
        var exists = await _db.Queryable<User>().AnyAsync(u => u.Phone == dto.Phone && !u.IsDeleted);
        if (exists)
            return ApiResult<LoginResultDto>.Fail("该手机号已注册");

        var user = new User
        {
            Phone = dto.Phone,
            PasswordHash = HashPassword(dto.Password),
            NickName = dto.NickName,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _db.Insertable(user).ExecuteReturnIdentityAsync();
        user.Id = id;

        // 自动创建家庭
        var family = new Family
        {
            Name = $"{dto.NickName}的家庭",
            InviteCode = GenerateInviteCode(),
            CreatedAt = DateTime.UtcNow
        };
        var familyId = await _db.Insertable(family).ExecuteReturnIdentityAsync();

        await _db.Insertable(new FamilyMember
        {
            FamilyId = familyId,
            UserId = id,
            Role = 1,
            CreatedAt = DateTime.UtcNow
        }).ExecuteReturnIdentityAsync();

        var token = GenerateToken(user);
        return ApiResult<LoginResultDto>.Success(new LoginResultDto
        {
            Token = token,
            NickName = user.NickName,
            Phone = user.Phone,
            UserId = user.Id
        });
    }

    public async Task<ApiResult<LoginResultDto>> LoginAsync(LoginDto dto)
    {
        var user = await _db.Queryable<User>()
            .FirstAsync(u => u.Phone == dto.Phone && !u.IsDeleted);

        if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
            return ApiResult<LoginResultDto>.Fail("手机号或密码错误");

        user.LastLoginAt = DateTime.UtcNow;
        await _db.Updateable(user).ExecuteCommandAsync();

        var token = GenerateToken(user);
        return ApiResult<LoginResultDto>.Success(new LoginResultDto
        {
            Token = token,
            NickName = user.NickName,
            Phone = user.Phone,
            UserId = user.Id
        });
    }

    private string GenerateToken(User user)
    {
        var jwtConfig = _config.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfig["Secret"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.NickName),
            new Claim("Phone", user.Phone)
        };

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }

    private static string GenerateInviteCode()
    {
        return Guid.NewGuid().ToString("N")[..8].ToUpper();
    }
}
