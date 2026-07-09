using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace SyncEdu.Core.Notifications;

/// <summary>
/// WebSocket 连接管理器 - 管理所有客户端连接
/// </summary>
public class NotificationHub
{
    private static readonly ConcurrentDictionary<string, WebSocket> _connections = new();
    private static readonly ConcurrentDictionary<long, List<string>> _userConnections = new();

    /// <summary>添加连接</summary>
    public static void AddConnection(string connectionId, WebSocket socket, long userId)
    {
        _connections.TryAdd(connectionId, socket);
        _userConnections.AddOrUpdate(userId,
            _ => new List<string> { connectionId },
            (_, list) => { lock (list) { list.Add(connectionId); } return list; });
    }

    /// <summary>移除连接</summary>
    public static async Task RemoveConnectionAsync(string connectionId)
    {
        if (_connections.TryRemove(connectionId, out var socket))
        {
            // 从用户连接列表中移除
            foreach (var kvp in _userConnections)
            {
                lock (kvp.Value)
                {
                    kvp.Value.Remove(connectionId);
                }
            }

            if (socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
                catch { }
            }
        }
    }

    /// <summary>向指定用户发送消息</summary>
    public static async Task SendToUserAsync(long userId, object message)
    {
        if (!_userConnections.TryGetValue(userId, out var connectionIds)) return;

        List<string> ids;
        lock (connectionIds)
        {
            ids = connectionIds.ToList();
        }

        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var buffer = Encoding.UTF8.GetBytes(json);

        foreach (var id in ids)
        {
            if (_connections.TryGetValue(id, out var socket) && socket.State == WebSocketState.Open)
            {
                try
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    await RemoveConnectionAsync(id);
                }
            }
        }
    }

    /// <summary>向所有连接广播消息</summary>
    public static async Task BroadcastAsync(object message)
    {
        var json = JsonSerializer.Serialize(message, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        var buffer = Encoding.UTF8.GetBytes(json);

        foreach (var kvp in _connections)
        {
            if (kvp.Value.State == WebSocketState.Open)
            {
                try
                {
                    await kvp.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                catch
                {
                    await RemoveConnectionAsync(kvp.Key);
                }
            }
        }
    }

    /// <summary>获取在线连接数</summary>
    public static int GetConnectionCount() => _connections.Count;

    /// <summary>获取在线用户数</summary>
    public static int GetUserCount() => _userConnections.Count;
}

/// <summary>
/// 通知消息类型
/// </summary>
public class NotificationMessage
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public object? Data { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 通知服务 - 用于业务层发送通知
/// </summary>
public interface INotificationService
{
    /// <summary>发送打卡提醒</summary>
    Task SendCheckInReminderAsync(long userId, string childName);

    /// <summary>发送学习提醒</summary>
    Task SendStudyReminderAsync(long userId, string title, string? content);

    /// <summary>发送成就达成通知</summary>
    Task SendAchievementUnlockedAsync(long userId, string achievementName);

    /// <summary>发送积分奖励通知</summary>
    Task SendPointsAwardedAsync(long userId, int points, string reason);

    /// <summary>发送鼓励消息</summary>
    Task SendEncouragementAsync(long userId, string content);

    /// <summary>发送教育数据同步通知</summary>
    Task SendSyncNotificationAsync(string status, string? message);

    /// <summary>发送自定义通知</summary>
    Task SendAsync(long userId, string type, string title, string? content = null, object? data = null);
}

public class NotificationService : INotificationService
{
    public Task SendCheckInReminderAsync(long userId, string childName)
        => SendAsync(userId, "checkin_reminder", "打卡提醒", $"{childName}今天还没有打卡哦~");

    public Task SendStudyReminderAsync(long userId, string title, string? content)
        => SendAsync(userId, "study_reminder", title, content);

    public Task SendAchievementUnlockedAsync(long userId, string achievementName)
        => SendAsync(userId, "achievement", "成就达成!", $"恭喜获得「{achievementName}」成就");

    public Task SendPointsAwardedAsync(long userId, int points, string reason)
        => SendAsync(userId, "points", "积分奖励", $"+{points} 积分", new { points, reason });

    public Task SendEncouragementAsync(long userId, string content)
        => SendAsync(userId, "encouragement", "鼓励", content);

    public Task SendSyncNotificationAsync(string status, string? message)
    {
        var notification = new NotificationMessage
        {
            Type = "sync",
            Title = "数据同步",
            Content = message ?? status,
            Data = new { status }
        };
        return NotificationHub.BroadcastAsync(notification);
    }

    public Task SendAsync(long userId, string type, string title, string? content = null, object? data = null)
    {
        var notification = new NotificationMessage
        {
            Type = type,
            Title = title,
            Content = content,
            Data = data
        };
        return NotificationHub.SendToUserAsync(userId, notification);
    }
}
