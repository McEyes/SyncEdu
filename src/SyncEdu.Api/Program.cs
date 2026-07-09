using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using SyncEdu.Core.Interfaces;
using SyncEdu.Core.Services;
using SyncEdu.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ===== Serilog 日志 =====
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// ===== SqlSugar + PostgreSQL =====
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? "Host=localhost;Port=5432;Database=syncedu;Username=postgres;Password=postgres";
builder.Services.AddSqlSugarSetup(connectionString);

// ===== JWT 认证 =====
var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["Secret"] ?? "SyncEdu_SuperSecretKey_2024_MustBeAtLeast32Chars!";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtConfig["Issuer"] ?? "SyncEdu",
            ValidAudience = jwtConfig["Audience"] ?? "SyncEdu",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();

// ===== 业务服务注册 =====
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFamilyService, FamilyService>();
builder.Services.AddScoped<IChildService, ChildService>();
builder.Services.AddScoped<IEducationService, EducationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// Phase 2 服务
builder.Services.AddScoped<ILearningService, LearningService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<IAchievementService, AchievementService>();
builder.Services.AddScoped<IEducationSyncService, EducationSyncService>();

// 教育数据提供者（预留）
builder.Services.AddScoped<IEducationDataProvider, SmartEducationPlatformProvider>();
builder.Services.AddScoped<IEducationDataProvider, LocalEducationBureauProvider>();

// ===== Controllers & Swagger =====
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SyncEdu API",
        Version = "v1",
        Description = "阳光同步学 API - 从幼儿园到大学的同步学习平台"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ===== CORS =====
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== 中间件管道 =====
// Swagger 始终可用
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SyncEdu API v1");
    c.RoutePrefix = "swagger";
});

app.UseSerilogRequestLogging();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ===== 自动建表（开发环境） =====
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SqlSugar.ISqlSugarClient>();
    // 开发阶段自动建表，生产环境应使用迁移
    db.CodeFirst.InitTables(
        typeof(SyncEdu.Core.Entities.User),
        typeof(SyncEdu.Core.Entities.Family),
        typeof(SyncEdu.Core.Entities.FamilyMember),
        typeof(SyncEdu.Core.Entities.Child),
        typeof(SyncEdu.Core.Entities.ChildProfile),
        typeof(SyncEdu.Core.Entities.EducationStage),
        typeof(SyncEdu.Core.Entities.Grade),
        typeof(SyncEdu.Core.Entities.Subject),
        typeof(SyncEdu.Core.Entities.TextbookVersion),
        typeof(SyncEdu.Core.Entities.ChildSubjectConfig),
        typeof(SyncEdu.Core.Entities.Textbook),
        typeof(SyncEdu.Core.Entities.Chapter),
        typeof(SyncEdu.Core.Entities.Lesson),
        typeof(SyncEdu.Core.Entities.LearningPlan),
        typeof(SyncEdu.Core.Entities.LearningProgress),
        typeof(SyncEdu.Core.Entities.StudySession),
        typeof(SyncEdu.Core.Entities.CheckIn),
        typeof(SyncEdu.Core.Entities.StudyReminder),
        typeof(SyncEdu.Core.Entities.Achievement),
        typeof(SyncEdu.Core.Entities.ChildAchievement),
        typeof(SyncEdu.Core.Entities.EncouragementLog),
        typeof(SyncEdu.Core.Entities.PointsTransaction)
    );

    // 初始化教育阶段基础数据
    var stageCount = db.Queryable<SyncEdu.Core.Entities.EducationStage>().Count();
    if (stageCount == 0)
    {
        var stages = new[]
        {
            new SyncEdu.Core.Entities.EducationStage { Name = "幼儿园", SortOrder = 1 },
            new SyncEdu.Core.Entities.EducationStage { Name = "小学", SortOrder = 2 },
            new SyncEdu.Core.Entities.EducationStage { Name = "初中", SortOrder = 3 },
            new SyncEdu.Core.Entities.EducationStage { Name = "高中", SortOrder = 4 },
            new SyncEdu.Core.Entities.EducationStage { Name = "大学", SortOrder = 5 }
        };
        db.Insertable(stages).ExecuteCommand();

        // 初始化年级数据
        var primaryGrades = new[]
        {
            new SyncEdu.Core.Entities.Grade { StageId = 2, Name = "一年级", SortOrder = 1 },
            new SyncEdu.Core.Entities.Grade { StageId = 2, Name = "二年级", SortOrder = 2 },
            new SyncEdu.Core.Entities.Grade { StageId = 2, Name = "三年级", SortOrder = 3 },
            new SyncEdu.Core.Entities.Grade { StageId = 2, Name = "四年级", SortOrder = 4 },
            new SyncEdu.Core.Entities.Grade { StageId = 2, Name = "五年级", SortOrder = 5 },
            new SyncEdu.Core.Entities.Grade { StageId = 2, Name = "六年级", SortOrder = 6 }
        };
        var juniorGrades = new[]
        {
            new SyncEdu.Core.Entities.Grade { StageId = 3, Name = "初一", SortOrder = 1 },
            new SyncEdu.Core.Entities.Grade { StageId = 3, Name = "初二", SortOrder = 2 },
            new SyncEdu.Core.Entities.Grade { StageId = 3, Name = "初三", SortOrder = 3 }
        };
        var seniorGrades = new[]
        {
            new SyncEdu.Core.Entities.Grade { StageId = 4, Name = "高一", SortOrder = 1 },
            new SyncEdu.Core.Entities.Grade { StageId = 4, Name = "高二", SortOrder = 2 },
            new SyncEdu.Core.Entities.Grade { StageId = 4, Name = "高三", SortOrder = 3 }
        };
        db.Insertable(primaryGrades).ExecuteCommand();
        db.Insertable(juniorGrades).ExecuteCommand();
        db.Insertable(seniorGrades).ExecuteCommand();

        // 初始化教材版本
        var versions = new[]
        {
            new SyncEdu.Core.Entities.TextbookVersion { Name = "人教版", Publisher = "人民教育出版社" },
            new SyncEdu.Core.Entities.TextbookVersion { Name = "北师大版", Publisher = "北京师范大学出版社" },
            new SyncEdu.Core.Entities.TextbookVersion { Name = "苏教版", Publisher = "江苏教育出版社" },
            new SyncEdu.Core.Entities.TextbookVersion { Name = "沪教版", Publisher = "上海教育出版社" },
            new SyncEdu.Core.Entities.TextbookVersion { Name = "外研版", Publisher = "外语教学与研究出版社" }
        };
        db.Insertable(versions).ExecuteCommand();

        // 初始化基础学科
        var subjects = new[]
        {
            new SyncEdu.Core.Entities.Subject { Name = "语文", SortOrder = 1 },
            new SyncEdu.Core.Entities.Subject { Name = "数学", SortOrder = 2 },
            new SyncEdu.Core.Entities.Subject { Name = "英语", SortOrder = 3 },
            new SyncEdu.Core.Entities.Subject { Name = "物理", SortOrder = 4 },
            new SyncEdu.Core.Entities.Subject { Name = "化学", SortOrder = 5 },
            new SyncEdu.Core.Entities.Subject { Name = "生物", SortOrder = 6 },
            new SyncEdu.Core.Entities.Subject { Name = "历史", SortOrder = 7 },
            new SyncEdu.Core.Entities.Subject { Name = "地理", SortOrder = 8 },
            new SyncEdu.Core.Entities.Subject { Name = "政治", SortOrder = 9 }
        };
        db.Insertable(subjects).ExecuteCommand();
    }
}

app.Run();
