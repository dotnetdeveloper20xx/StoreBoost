using StoreBoost.Application.Interfaces;
using System;


namespace StoreBoost.Infrastructure.Services
{
    /// <summary>
    /// Simple mock notification service. In real-world scenarios, this could email, SMS, or push.
    /// </summary>
    public class FakeNotificationService : INotificationService
    {
        public Task SendAsync(Guid userId, string message)
        {
            Console.WriteLine($"📣 [Notification] To: {userId} → {message}");
            return Task.CompletedTask;
        }
    }
}
