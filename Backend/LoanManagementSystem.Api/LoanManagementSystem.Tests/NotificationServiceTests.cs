using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LoanManagementSystem.Api.Models;
using LoanManagementSystem.Api.Repositories.Interfaces;
using LoanManagementSystem.Api.Services.Implementations;
using Moq;
using Xunit;

namespace LoanManagementSystem.Tests.Services
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _mockRepo;
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly NotificationService _service;

        public NotificationServiceTests()
        {
            _mockRepo = new Mock<INotificationRepository>();
            _mockUserRepo = new Mock<IUserRepository>();
            _service = new NotificationService(_mockRepo.Object, _mockUserRepo.Object);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnNotifications_WhenCalled()
        {
            // Arrange
            var userId = 1;
            var notifications = new List<Notification>
            {
                new Notification { UserId = userId, Message = "Test 1" },
                new Notification { UserId = userId, Message = "Test 2" }
            };

            _mockRepo.Setup(r => r.GetUserNotificationsAsync(userId))
                     .ReturnsAsync(notifications);

            // Act
            var result = await _service.GetUserNotificationsAsync(userId);

            // Assert
            Assert.Equal(2, ((List<Notification>)result).Count);
            _mockRepo.Verify(r => r.GetUserNotificationsAsync(userId), Times.Once);
        }

        [Fact]
        public async Task CreateNotificationAsync_ShouldAddNotification_WhenCalled()
        {
            // Arrange
            var userId = 1;
            var message = "Test Message";
            var type = "Info";

            // Act
            await _service.CreateNotificationAsync(userId, message, type);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(It.Is<Notification>(n => 
                n.UserId == userId && 
                n.Message == message && 
                n.Type == type &&
                n.IsRead == false
            )), Times.Once);
        }

        [Fact]
        public async Task NotifyRoleAsync_ShouldCreateNotificationsForUsersInRole()
        {
            // Arrange
            var role = "Admin";
            var message = "Hello Admin";
            var users = new List<User>
            {
                new User { UserId = 10, Role = role },
                new User { UserId = 11, Role = role }
            };

            _mockUserRepo.Setup(u => u.GetUsersByRoleAsync(role))
                         .ReturnsAsync(users);

            // Act
            await _service.NotifyRoleAsync(role, message);

            // Assert
            _mockRepo.Verify(r => r.AddAsync(It.Is<Notification>(n => n.UserId == 10 && n.Message == message)), Times.Once);
            _mockRepo.Verify(r => r.AddAsync(It.Is<Notification>(n => n.UserId == 11 && n.Message == message)), Times.Once);
            _mockRepo.Verify(r => r.AddAsync(It.IsAny<Notification>()), Times.Exactly(2));
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldUpdateNotification_WhenExists()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            var notification = new Notification { Id = notificationId, IsRead = false };

            _mockRepo.Setup(r => r.GetByIdAsync(notificationId))
                     .ReturnsAsync(notification);

            // Act
            await _service.MarkAsReadAsync(notificationId);

            // Assert
            Assert.True(notification.IsRead);
            _mockRepo.Verify(r => r.UpdateAsync(notification), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldDoNothing_WhenNotificationNotFound()
        {
            // Arrange
            var notificationId = Guid.NewGuid();
            _mockRepo.Setup(r => r.GetByIdAsync(notificationId))
                     .ReturnsAsync((Notification?)null);

            // Act
            await _service.MarkAsReadAsync(notificationId);

            // Assert
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<Notification>()), Times.Never);
        }

        [Fact]
        public async Task MarkAllAsReadAsync_ShouldCallRepository_WhenCalled()
        {
            // Arrange
            var userId = 1;

            // Act
            await _service.MarkAllAsReadAsync(userId);

            // Assert
            _mockRepo.Verify(r => r.MarkAllAsReadAsync(userId), Times.Once);
        }
    }
}
