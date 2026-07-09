using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SyncEdu.Core.Notifications;

namespace SyncEdu.Api.Controllers;

/// <summary>
/// WebSocket 通知端点
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationController : ControllerBase
{
    /// <summary>WebSocket 连接端点</summary>
    [HttpGet("ws")]
    [Authorize]
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = 400;
            return;
        }

        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var connectionId = Guid.NewGuid().ToString("N");

        var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        NotificationHub.AddConnection(connectionId, socket, userId);

        // 发送连接成功消息
        var welcomeMsg = new NotificationMessage
        {
            Type = "connected",
            Title = "连接成功",
            Content = $"WebSocket 已连接，连接ID: {connectionId}"
        };
        await NotificationHub.SendToUserAsync(userId, welcomeMsg);

        // 保持连接，接收客户端消息（如心跳）
        var buffer = new byte[1024 * 4];
        try
        {
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                // 处理客户端发来的消息（如心跳 pong）
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);
                    if (message == "ping")
                    {
                        var pong = System.Text.Encoding.UTF8.GetBytes("pong");
                        await socket.SendAsync(new ArraySegment<byte>(pong), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
        finally
        {
            await NotificationHub.RemoveConnectionAsync(connectionId);
        }
    }

    /// <summary>获取在线状态</summary>
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            connections = NotificationHub.GetConnectionCount(),
            users = NotificationHub.GetUserCount(),
            webSocketSupported = HttpContext.WebSockets.IsWebSocketRequest
        });
    }

    /// <summary>发送测试通知</summary>
    [HttpPost("test")]
    [Authorize]
    public async Task<IActionResult> SendTest([FromBody] TestNotificationDto dto)
    {
        var userId = long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var notification = new NotificationMessage
        {
            Type = dto.Type ?? "test",
            Title = dto.Title ?? "测试通知",
            Content = dto.Content ?? "这是一条测试通知消息"
        };
        await NotificationHub.SendToUserAsync(userId, notification);
        return Ok(new { message = "通知已发送" });
    }

    /// <summary>广播通知（管理员）</summary>
    [HttpPost("broadcast")]
    [Authorize]
    public async Task<IActionResult> Broadcast([FromBody] TestNotificationDto dto)
    {
        var notification = new NotificationMessage
        {
            Type = dto.Type ?? "broadcast",
            Title = dto.Title ?? "系统通知",
            Content = dto.Content
        };
        await NotificationHub.BroadcastAsync(notification);
        return Ok(new { message = "广播已发送" });
    }
}

public class TestNotificationDto
{
    public string? Type { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
}
