using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SalesService.Application.Interfaces;
using SalesService.Application.DTOs;

namespace SalesService.Api.Services
{
    public class NotificationBackgroundService : BackgroundService
    {
        private readonly ILogger<NotificationBackgroundService> _logger;
        private readonly SalesService.Application.Services.INotificationQueueService _notificationQueueService;
        private readonly IServiceScopeFactory _scopeFactory;

        public NotificationBackgroundService(ILogger<NotificationBackgroundService> logger, SalesService.Application.Services.INotificationQueueService notificationQueueService, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _notificationQueueService = notificationQueueService;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Notification Background Service running.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        _logger.LogInformation("Checking for pending notifications from queue...");
                        var notification = await _notificationQueueService.DequeueAsync(stoppingToken);
                        _logger.LogInformation("Processing notification from queue: {NotificationId} - {Message}", notification.Id, notification.Message);
                        // Here you would integrate with an actual email sending service
                        // For now, we'll just log it
                        _logger.LogInformation("Simulating sending email for notification {NotificationId}", notification.Id);
                        // In a real scenario, after successful sending, you might mark it as sent in a database
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in Notification Background Service.");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Check every minute
            }

            _logger.LogInformation("Notification Background Service stopped.");
        }
    }
}