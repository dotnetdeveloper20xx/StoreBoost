using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Queries.GetAvailable;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Queries.GetAvailable
{
    public class GetAvailableSlotsQueryHandlerTests
    {
        [Fact]
        public async Task Should_Return_Only_Available_Slots_With_Expected_Booking_Info()
        {
            // Arrange
            var mockRepo = new Mock<ISlotRepository>();

            // Slot A: 1 of 3 booked → available
            var slot1 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(9), maxBookings: 3);
            slot1.Book();

            // Slot B: 2 of 2 booked → not available, should NOT be in repo return
            var slot2 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(10), maxBookings: 2);
            slot2.Book();
            slot2.Book();

            var availableSlots = new List<AppointmentSlot> { slot1 };

            mockRepo.Setup(r => r.GetAvailableAsync()).ReturnsAsync(availableSlots);

            var handler = new GetAvailableSlotsQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(new GetAvailableSlotsQuery(), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);

            var dto = result.Data!.First();
            dto.IsBooked.Should().BeFalse();
            dto.MaxBookings.Should().Be(3);
            dto.CurrentBookings.Should().Be(1);
        }
    }
}
