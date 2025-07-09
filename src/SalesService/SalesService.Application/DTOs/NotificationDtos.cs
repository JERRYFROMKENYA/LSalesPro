namespace SalesService.Application.DTOs;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string Priority { get; set; } = "Normal";
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public bool IsSent { get; set; }
    public DateTime? SentAt { get; set; }
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Data { get; set; }
}

public class CreateNotificationDto
{
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RecipientId { get; set; }
    public string? RecipientEmail { get; set; }
    public string Priority { get; set; } = "Normal";
    public string? Data { get; set; }
}
