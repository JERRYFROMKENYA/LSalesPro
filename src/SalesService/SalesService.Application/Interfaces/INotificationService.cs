using SalesService.Application.DTOs;

namespace SalesService.Application.Interfaces
{
    public interface INotificationService
    {
        Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto);
        Task<NotificationDto?> GetNotificationByIdAsync(Guid id);
        Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, bool? isRead = null);
        Task MarkNotificationAsReadAsync(Guid id);
        Task<int> GetUnreadNotificationsCountAsync(string userId);
        Task EnqueueNotificationAsync(NotificationDto notification);
    }
}