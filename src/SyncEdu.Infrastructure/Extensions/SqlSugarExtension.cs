using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;

namespace SyncEdu.Infrastructure.Extensions;

public static class SqlSugarExtension
{
    public static IServiceCollection AddSqlSugarSetup(this IServiceCollection services, string connectionString)
    {
        var configConnection = new ConnectionConfig()
        {
            DbType = DbType.PostgreSQL,
            ConnectionString = connectionString,
            IsAutoCloseConnection = true,
            InitKeyType = InitKeyType.Attribute,
            MoreSettings = new ConnMoreSettings()
            {
                IsAutoRemoveDataCache = true
            },
            ConfigureExternalServices = new ConfigureExternalServices()
            {
                EntityService = (c, p) =>
                {
                    // 处理可空类型
                    var nullability = new NullabilityInfoContext().Create(c);
                    if (nullability.ReadState is NullabilityState.Nullable)
                    {
                        p.IsNullable = true;
                    }
                }
            }
        };

        var sqlSugarClient = new SqlSugarScope(configConnection, db =>
        {
            // SQL执行前
            db.Aop.OnLogExecuting = (sql, pars) =>
            {
                // 开发环境打印SQL
                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    Console.WriteLine($"[SqlSugar SQL] {sql}");
                }
            };

            // 异常处理
            db.Aop.OnError = ex =>
            {
                Console.WriteLine($"[SqlSugar Error] {ex.Message}");
            };
        });

        services.AddSingleton<ISqlSugarClient>(sqlSugarClient);
        services.AddScoped(typeof(Repository<>));

        return services;
    }
}

/// <summary>
/// 泛型仓储基类
/// </summary>
public class Repository<T> : SimpleClient<T> where T : class, new()
{
    public Repository(ISqlSugarClient context) : base(context)
    {
    }

    // 可以在这里扩展通用方法
    public new async Task<List<T>> GetListAsync()
    {
        return await base.GetListAsync();
    }
}
