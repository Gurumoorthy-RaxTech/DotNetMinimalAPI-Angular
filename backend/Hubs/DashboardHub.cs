using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MinimalApiJwt.Models;

namespace MinimalApiJwt.Hubs;

// ============================================================
// DASHBOARD HUB - Real-time communication center
// Tanglish: Idhu server <-> client real-time talk pannuvom
// Client connect aana udane OnConnectedAsync call aagum
// Client disconnect aana OnDisconnectedAsync call aagum
// ============================================================

[Authorize] // JWT token mandatory for SignalR connection
public class DashboardHub : Hub
{
    private static int _connectedCount = 0;
    private readonly ILogger<DashboardHub> _logger;

    public DashboardHub(ILogger<DashboardHub> logger)
    {
        _logger = logger;
    }

    // Client connect aana udane call aagum
    public override async Task OnConnectedAsync()
    {
        _connectedCount++;
        var username = Context.User?.Identity?.Name ?? "Anonymous";

        _logger.LogInformation("User {User} connected. Total: {Count}", username, _connectedCount);

        // All clients ku connected count update pannuvom
        await Clients.All.SendAsync("ConnectedUsersUpdated", _connectedCount);

        // Only this client ku welcome message
        await Clients.Caller.SendAsync("ReceiveNotification", new Notification
        {
            Title = "Connected!",
            Message = $"Welcome {username}! Real-time dashboard active.",
            Type = "success"
        });

        // All others ku join notification
        await Clients.Others.SendAsync("ReceiveNotification", new Notification
        {
            Title = "User Joined",
            Message = $"{username} joined the dashboard",
            Type = "info"
        });

        await base.OnConnectedAsync();
    }

    // Client disconnect aana call aagum
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connectedCount = Math.Max(0, _connectedCount - 1);
        var username = Context.User?.Identity?.Name ?? "Anonymous";

        _logger.LogInformation("User {User} disconnected. Total: {Count}", username, _connectedCount);

        await Clients.All.SendAsync("ConnectedUsersUpdated", _connectedCount);

        await Clients.Others.SendAsync("ReceiveNotification", new Notification
        {
            Title = "User Left",
            Message = $"{username} left the dashboard",
            Type = "warning"
        });

        await base.OnDisconnectedAsync(exception);
    }

    // Client -> Server method: ping pannuvom
    // Angular: connection.invoke("Ping", "hello")
    public async Task Ping(string message)
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        // Server -> Client response
        await Clients.Caller.SendAsync("Pong", $"Server received: {message} from {username}");
    }

    // Client -> Server: broadcast message to all
    public async Task BroadcastMessage(string message)
    {
        var username = Context.User?.Identity?.Name ?? "Anonymous";
        await Clients.All.SendAsync("ReceiveMessage", new
        {
            User = username,
            Message = message,
            Time = DateTime.Now.ToString("HH:mm:ss")
        });
    }

    // Static method - other services call panlam (StudentService, etc.)
    public static int GetConnectedCount() => _connectedCount;
}
