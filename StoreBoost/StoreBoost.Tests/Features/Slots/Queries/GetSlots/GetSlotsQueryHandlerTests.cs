using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Queries.GetSlots
{
    public class GetSlotsQueryHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly Mock<ILogger<GetSlotsQueryHandler>> _mockLogger;
        private readonly GetSlotsQueryHandler _handler;

        public GetSlotsQueryHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _mockLogger = new Mock<ILogger<GetSlotsQueryHandler>>();

            _handler = new GetSlotsQueryHandler(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Should_Return_All_Slots_With_Accurate_Booking_Info()
        {
            // Arrange
            var slot1 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(9), maxBookings: 3);
            slot1.Book(); // CurrentBookings = 1

            var slot2 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(10), maxBookings: 2);
            slot2.Book(); slot2.Book(); // Fully booked

            _mockRepository.Setup(r => r.GetAllAsync())
                           .ReturnsAsync(new List<AppointmentSlot> { slot1, slot2 });

            // Act
            var result = await _handler.Handle(new GetSlotsQuery(), CancellationToken.None);

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

        [Fact]
        public async Task Should_Return_Failure_If_Exception_Occurs()
        {
            _mockRepository.Setup(r => r.GetAllAsync())
                           .ThrowsAsync(new Exception("DB failure"));

            var result = await _handler.Handle(new GetSlotsQuery(), CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred while retrieving slots.");

            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error occurred while retrieving all slots")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }
    }
}
