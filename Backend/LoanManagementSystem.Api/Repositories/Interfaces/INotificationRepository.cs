using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Repositories.Interfaces
{
    public interface INotificationRepository
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        Task<Notification?> GetByIdAsync(Guid id);
        Task AddAsync(Notification notification);
        Task UpdateAsync(Notification notification);
        Task MarkAllAsReadAsync(int userId);
    }
}
