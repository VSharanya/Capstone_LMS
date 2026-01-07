using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanManagementSystem.Api.Models;

namespace LoanManagementSystem.Api.Services.Interfaces
{
    public interface INotificationService
    {
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId);
        Task CreateNotificationAsync(int userId, string message, string type = "Info");
        Task NotifyRoleAsync(string role, string message, string type = "Info");
        Task MarkAsReadAsync(Guid id);
        Task MarkAllAsReadAsync(int userId);
    }
}
