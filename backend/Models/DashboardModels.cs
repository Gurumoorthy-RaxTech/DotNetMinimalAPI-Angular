namespace MinimalApiJwt.Models;

// Real-time dashboard stats - SignalR la broadcast pannuvom
public class DashboardStats
{
    public int TotalStudents { get; set; }
    public int ConnectedUsers { get; set; }
    public int TotalCourses { get; set; }
    public string ServerTime { get; set; } = string.Empty;
    public double CpuUsage { get; set; }
    public long MemoryUsageMb { get; set; }
}

// Activity log entry - student add/edit/delete events
public class ActivityLog
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Action { get; set; } = string.Empty;    // "ADDED" | "UPDATED" | "DELETED"
    public string Message { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
    public string Type { get; set; } = "info";            // "success" | "warning" | "danger" | "info"
}

// Notification - real-time alert
public class Notification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info";
    public string Timestamp { get; set; } = DateTime.Now.ToString("HH:mm:ss");
}

// Chart data point - for live chart
public class ChartDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Value { get; set; }
}
