using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.CancelSlotBooking;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Commands.CancelSlotBooking
{
    public class CancelSlotBookingCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly CancelSlotBookingCommandHandler _handler;

        public CancelSlotBookingCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _handler = new CancelSlotBookingCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Not_Found()
        {
            // Arrange
            var command = new CancelSlotBookingCommand(Guid.NewGuid());

            _mockRepository.Setup(repo => repo.GetByIdAsync(command.SlotId))
                .ReturnsAsync((AppointmentSlot?)null);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot not found.");
        }

        [Fact]
        public async Task Should_Return_Failure_If_No_Bookings_To_Cancel()
        {
            // Arrange
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);
            var command = new CancelSlotBookingCommand(slot.Id);

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id))
                .ReturnsAsync(slot);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("No bookings exist to cancel for this slot.");
        }

        [Fact]
        public async Task Should_Return_Success_When_Cancellation_Is_Valid()
        {
            // Arrange
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 3);
            slot.Book(); // CurrentBookings = 1

            var command = new CancelSlotBookingCommand(slot.Id);

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(repo => repo.UpdateAsync(slot)).ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Booking successfully cancelled.");
            slot.CurrentBookings.Should().Be(0);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Repository_Update_Fails()
        {
            // Arrange
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);
            slot.Book(); // CurrentBookings = 1

            var command = new CancelSlotBookingCommand(slot.Id);

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(repo => repo.UpdateAsync(slot)).ReturnsAsync(false);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to persist slot cancellation.");
        }
    }
}
