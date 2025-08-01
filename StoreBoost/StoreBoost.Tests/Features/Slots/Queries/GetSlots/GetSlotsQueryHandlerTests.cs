using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Queries.GetSlots
{
    public class GetSlotsQueryHandlerTests
    {
        [Fact]
        public async Task Should_Return_All_Slots_With_Accurate_Booking_Info()
        {
            // Arrange
            var mockRepo = new Mock<ISlotRepository>();

            // Slot 1: Available (1/3 booked)
            var slot1 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(9), maxBookings: 3);
            slot1.Book(); // CurrentBookings = 1

            // Slot 2: Fully Booked (2/2)
            var slot2 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(10), maxBookings: 2);
            slot2.Book();
            slot2.Book(); // CurrentBookings = 2

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<AppointmentSlot> { slot1, slot2 });

            var handler = new GetSlotsQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(new GetSlotsQuery(), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);

            var dto1 = result.Data!.First(s => s.Id == slot1.Id);
            dto1.CurrentBookings.Should().Be(1);
            dto1.MaxBookings.Should().Be(3);
            dto1.IsBooked.Should().BeFalse();

            var dto2 = result.Data!.First(s => s.Id == slot2.Id);
            dto2.CurrentBookings.Should().Be(2);
            dto2.MaxBookings.Should().Be(2);
            dto2.IsBooked.Should().BeTrue();
        }
    }
}
