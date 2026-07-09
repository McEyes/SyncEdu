using SyncEdu.Core.Interfaces;
using SyncEdu.Infrastructure.Extensions;
using SyncEdu.Job;

var builder = Host.CreateApplicationBuilder(args);

// SqlSugar + PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL")
    ?? "Host=localhost;Port=5432;Database=syncedu;Username=postgres;Password=postgres";
builder.Services.AddSqlSugarSetup(connectionString);

// 教育数据提供者
builder.Services.AddScoped<IEducationDataProvider, SmartEducationPlatformProvider>();
builder.Services.AddScoped<IEducationDataProvider, LocalEducationBureauProvider>();

// 定时任务
builder.Services.AddHostedService<EducationSyncWorker>();
builder.Services.AddHostedService<ReminderPushWorker>();

var host = builder.Build();
host.Run();
