using Microsoft.AspNetCore.Mvc;
using SalesService.Application.DTOs;
using SalesService.Application.Interfaces;

namespace SalesService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Get user notifications with optional filter for read status.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="isRead">Optional. Filter by read status.</param>
        /// <returns>A list of notifications.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetUserNotifications([FromQuery] string userId, [FromQuery] bool? isRead = null)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, isRead);
            return Ok(notifications);
        }

        /// <summary>
        /// Mark a notification as read.
        /// </summary>
        /// <param name="id">The ID of the notification to mark as read.</param>
        /// <returns>No content if successful.</returns>
        [HttpPut("{id}/read")]
        public async Task<IActionResult> MarkNotificationAsRead(Guid id)
        {
            await _notificationService.MarkNotificationAsReadAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Get the count of unread notifications for a user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The count of unread notifications.</returns>
        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadNotificationsCount([FromQuery] string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("User ID is required.");
            }

            var count = await _notificationService.GetUnreadNotificationsCountAsync(userId);
            return Ok(count);
        }
    }
}