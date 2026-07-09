namespace SyncEdu.Shared.Models;

/// <summary>
/// 统一API响应
/// </summary>
public class ApiResult<T>
{
    public int Code { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResult<T> Success(T data, string message = "操作成功")
        => new() { Code = 0, Message = message, Data = data };

    public static ApiResult<T> Fail(string message, int code = -1)
        => new() { Code = code, Message = message };
}

public class ApiResult : ApiResult<object>
{
    public static ApiResult Success(string message = "操作成功")
        => new() { Code = 0, Message = message };

    public new static ApiResult Fail(string message, int code = -1)
        => new() { Code = code, Message = message };
}

/// <summary>
/// 分页请求
/// </summary>
public class PageRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// 分页响应
/// </summary>
public class PageResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Total { get; set; }
    public int PageIndex { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
}
