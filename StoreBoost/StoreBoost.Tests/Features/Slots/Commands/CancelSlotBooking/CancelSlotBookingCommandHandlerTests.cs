using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.CancelSlotBooking;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Commands.CancelSlotBooking
{
    public class CancelSlotBookingCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly Mock<INotificationService> _mockNotifier;
        private readonly CancelSlotBookingCommandHandler _handler;

        public CancelSlotBookingCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _mockNotifier = new Mock<INotificationService>();
            _handler = new CancelSlotBookingCommandHandler(_mockRepository.Object, _mockNotifier.Object);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Not_Found()
        {
            var command = new CancelSlotBookingCommand(Guid.NewGuid());

            _mockRepository.Setup(repo => repo.GetByIdAsync(command.SlotId))
                .ReturnsAsync((AppointmentSlot?)null);

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot not found.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Return_Failure_If_No_Bookings_To_Cancel()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);
            var command = new CancelSlotBookingCommand(slot.Id);

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id)).ReturnsAsync(slot);

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("No bookings exist to cancel for this slot.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Send_Notification_When_Cancellation_Is_Successful()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 3);
            slot.Book(); // 1 booking to cancel

            var command = new CancelSlotBookingCommand(slot.Id);

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(repo => repo.UpdateAsync(slot)).ReturnsAsync(true);

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            result.Message.Should().Be("Booking successfully cancelled.");
            slot.CurrentBookings.Should().Be(0);

            _mockNotifier.Verify(n =>
                n.SendAsync(It.IsAny<Guid>(), It.Is<string>(msg => msg.Contains("cancelled"))),
                Times.Once);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Repository_Update_Fails()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);
            slot.Book();

            var command = new CancelSlotBookingCommand(slot.Id);

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(repo => repo.UpdateAsync(slot)).ReturnsAsync(false);

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to persist slot cancellation.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }
    }
}
