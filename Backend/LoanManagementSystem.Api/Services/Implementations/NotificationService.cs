using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Interfaces;

namespace LoanManagementSystem.Api.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _repository;
        private readonly IUserRepository _userRepository;

        public NotificationService(INotificationRepository repository, IUserRepository userRepository)
        {
            _repository = repository;
            _userRepository = userRepository;
        }

        // Retrieves all notifications belonging to a specific user.
        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId)
        {
            return await _repository.GetUserNotificationsAsync(userId);
        }

        // Creates a new notification record for a user.
        public async Task CreateNotificationAsync(int userId, string message, string type = "Info")
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(notification);
        }

        // Sends a notification to all users matching a specific role (e.g., Admin, LoanOfficer).
        public async Task NotifyRoleAsync(string role, string message, string type = "Info")
        {
            var users = await _userRepository.GetUsersByRoleAsync(role);
            foreach (var user in users)
            {
                await CreateNotificationAsync(user.UserId, message, type);
            }
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var notification = await _repository.GetByIdAsync(id);
            if (notification != null)
            {
                notification.IsRead = true;
                await _repository.UpdateAsync(notification);
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            await _repository.MarkAllAsReadAsync(userId);
        }
    }
}
