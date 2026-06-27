using Microsoft.AspNetCore.SignalR;
using MinimalApiJwt.Hubs;
using MinimalApiJwt.Models;

namespace MinimalApiJwt.Services;

// ============================================================
// BACKGROUND SERVICE - Every second real-time data broadcast pannuvom
// Tanglish: Server background la run aagum, automatically
// stats update pannuvom - CPU, Memory, Time, Student count
// BackgroundService = .NET built-in long running service
// ============================================================
public class DashboardBackgroundService : BackgroundService
{
    private readonly IHubContext<DashboardHub> _hubContext;
    private readonly ILogger<DashboardBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly Random _random = new();

    public DashboardBackgroundService(
        IHubContext<DashboardHub> hubContext,
        ILogger<DashboardBackgroundService> logger,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    // ExecuteAsync - background task start aana udane run aagum
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Dashboard Background Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await BroadcastStats();
                await Task.Delay(2000, stoppingToken); // Every 2 seconds update
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in background service");
                await Task.Delay(5000, stoppingToken); // Error aana 5s wait
            }
        }
    }

    private async Task BroadcastStats()
    {
        // IServiceProvider la StudentService resolve pannuvom
        using var scope = _serviceProvider.CreateScope();
        var studentService = scope.ServiceProvider.GetRequiredService<IStudentService>();

        var students = studentService.GetAll();
        var courses = students.Select(s => s.Course).Distinct().Count();

        // Real system memory usage
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMb = process.WorkingSet64 / 1024 / 1024;

        // Simulated CPU (real CPU requires more complex calculation)
        var cpuUsage = Math.Round(_random.NextDouble() * 30 + 5, 1); // 5-35%

        var stats = new DashboardStats
        {
            TotalStudents = students.Count,
            ConnectedUsers = DashboardHub.GetConnectedCount(),
            TotalCourses = courses,
            ServerTime = DateTime.Now.ToString("HH:mm:ss"),
            CpuUsage = cpuUsage,
            MemoryUsageMb = memoryMb
        };

        // All connected clients ku stats broadcast pannuvom
        await _hubContext.Clients.All.SendAsync("StatsUpdated", stats);
    }

    // Other services call panlam - student add/delete events
    public async Task BroadcastActivity(ActivityLog activity)
    {
        await _hubContext.Clients.All.SendAsync("ActivityReceived", activity);
    }

    public async Task BroadcastNotification(Notification notification)
    {
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
    }
}
