using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Features.Slots.Queries.GetAvailable;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Queries.GetAvailable
{
    public class GetAvailableSlotsQueryHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly Mock<ILogger<GetAvailableSlotsQueryHandler>> _mockLogger;
        private readonly GetAvailableSlotsQueryHandler _handler;

        public GetAvailableSlotsQueryHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _mockLogger = new Mock<ILogger<GetAvailableSlotsQueryHandler>>();

            _handler = new GetAvailableSlotsQueryHandler(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Should_Return_Only_Available_Slots_With_Expected_Booking_Info()
        {
            // Arrange
            var slot1 = new AppointmentSlot(Guid.NewGuid(), DateTime.Today.AddHours(9), maxBookings: 3);
            slot1.Book(); // 1/3 → available

            _mockRepository.Setup(r => r.GetAvailableAsync()).ReturnsAsync(new List<AppointmentSlot> { slot1 });

            // Act
            var result = await _handler.Handle(new GetAvailableSlotsQuery(), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);

            var dto = result.Data!.First();
            dto.IsBooked.Should().BeFalse();
            dto.MaxBookings.Should().Be(3);
            dto.CurrentBookings.Should().Be(1);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Exception_Occurs()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAvailableAsync())
                           .ThrowsAsync(new Exception("Database failure"));

            // Act
            var result = await _handler.Handle(new GetAvailableSlotsQuery(), CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred while retrieving available slots.");

            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error retrieving available slots.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()
                ),
                Times.Once
            );
        }
    }
}
