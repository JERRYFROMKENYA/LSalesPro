using SalesService.Domain.Entities;

namespace SalesService.Domain.Interfaces
{
    public interface INotificationRepository : IGenericRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetByUserIdAsync(string userId);
        Task<int> GetUnreadCountByUserIdAsync(string userId);
    }
}