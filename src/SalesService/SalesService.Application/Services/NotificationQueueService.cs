using System.Threading.Channels;
using SalesService.Application.DTOs;

namespace SalesService.Application.Services
{
    public class NotificationQueueService : INotificationQueueService
    {
        private readonly Channel<NotificationDto> _queue;

        public NotificationQueueService()
        {
            // Bounded channel to prevent unbounded growth
            var options = new BoundedChannelOptions(capacity: 100) 
            {
                FullMode = BoundedChannelFullMode.Wait
            };
            _queue = Channel.CreateBounded<NotificationDto>(options);
        }

        public async ValueTask EnqueueAsync(NotificationDto notification)
        {
            await _queue.Writer.WriteAsync(notification);
        }

        public async ValueTask<NotificationDto> DequeueAsync(CancellationToken cancellationToken)
        {
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
    }

    public interface INotificationQueueService
    {
        ValueTask EnqueueAsync(NotificationDto notification);
        ValueTask<NotificationDto> DequeueAsync(CancellationToken cancellationToken);
    }
}