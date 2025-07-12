using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;
using SalesService.Domain.Entities;
using SalesService.Domain.Interfaces;

namespace SalesService.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly SalesService.Domain.Interfaces.INotificationRepository _notificationRepository;

        public NotificationService(SalesService.Domain.Interfaces.INotificationRepository notificationRepository)
        {
            _notificationRepository = notificationRepository;
        }

        public async Task<NotificationDto> CreateNotificationAsync(CreateNotificationDto notificationDto)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = notificationDto.UserId,
                Message = notificationDto.Message,
                Type = notificationDto.Type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            var createdNotification = await _notificationRepository.AddAsync(notification);
            return MapToDto(createdNotification);
        }

        public async Task<NotificationDto?> GetNotificationByIdAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            return notification != null ? MapToDto(notification) : null;
        }

        public async Task<IEnumerable<NotificationDto>> GetUserNotificationsAsync(string userId, bool? isRead = null)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);

            if (isRead.HasValue)
            {
                notifications = notifications.Where(n => n.IsRead == isRead.Value);
            }

            return notifications.Select(MapToDto);
        }

        public async Task MarkNotificationAsReadAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _notificationRepository.UpdateAsync(notification);
            }
        }

        public async Task<int> GetUnreadNotificationsCountAsync(string userId)
        {
            var count = await _notificationRepository.GetUnreadCountByUserIdAsync(userId);
            return count;
        }

        public async Task EnqueueNotificationAsync(NotificationDto notification)
        {
            // This should enqueue the notification for background processing
            // For now, throw NotImplementedException to indicate it needs a real implementation
            throw new NotImplementedException("Notification queueing is not implemented. Implement using INotificationQueueService if available.");
        }

        private static NotificationDto MapToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}