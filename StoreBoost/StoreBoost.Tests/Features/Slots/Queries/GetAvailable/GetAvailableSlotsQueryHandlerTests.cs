using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Queries.GetAvailable;
using StoreBoost.Application.Interfaces;
using StoreBoost.Domain.Entities;

namespace StoreBoost.Tests.Features.Slots.Queries.GetAvailable
{
    public class GetAvailableSlotsQueryHandlerTests
    {
        [Fact]
        public async Task Should_Return_Only_Available_Slots()
        {
            // Arrange
            var mockRepo = new Mock<ISlotRepository>();
            var availableSlot = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(9));
            var bookedSlot = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(10));
            bookedSlot.Book();

            var filtered = new List<AppointmentSlot> { availableSlot };

            mockRepo.Setup(r => r.GetAvailableAsync()).ReturnsAsync(filtered);

            var handler = new GetAvailableSlotsQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(new GetAvailableSlotsQuery(), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data!.First().IsBooked.Should().BeFalse();
        }
    }
}
