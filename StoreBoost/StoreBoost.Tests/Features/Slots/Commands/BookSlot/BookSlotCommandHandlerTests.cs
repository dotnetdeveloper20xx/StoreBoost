using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Unit tests for BookSlotCommandHandler with notification and booking logic.
    /// </summary>
    public class BookSlotCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly Mock<INotificationService> _mockNotifier;
        private readonly BookSlotCommandHandler _handler;

        public BookSlotCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _mockNotifier = new Mock<INotificationService>();

            _handler = new BookSlotCommandHandler(_mockRepository.Object, _mockNotifier.Object);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Not_Found()
        {
            _mockRepository.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((AppointmentSlot?)null);

            var command = new BookSlotCommand(Guid.NewGuid());

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot not found.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Is_Fully_Booked()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 2);
            slot.Book();
            slot.Book();

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);

            var command = new BookSlotCommand(slot.Id);

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot is fully booked.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Return_Success_And_Send_Notification_On_Booking()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 2);

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(r => r.UpdateAsync(slot)).ReturnsAsync(true);

            var command = new BookSlotCommand(slot.Id);
            var result = await _handler.Handle(command, default);

            result.Success.Should().BeTrue();
            result.Message.Should().Be("Slot booked successfully.");

            _mockNotifier.Verify(n =>
                n.SendAsync(It.IsAny<Guid>(), It.Is<string>(msg => msg.Contains("confirmed"))),
                Times.Once);
        }

        [Fact]
        public async Task Should_Send_Fully_Booked_Notification_When_Last_Spot_Is_Taken()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 2);
            slot.Book(); // already 1/2

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(r => r.UpdateAsync(slot)).ReturnsAsync(true);

            var command = new BookSlotCommand(slot.Id);
            var result = await _handler.Handle(command, default);

            result.Success.Should().BeTrue();

            _mockNotifier.Verify(n =>
                n.SendAsync(It.IsAny<Guid>(), It.Is<string>(msg => msg.Contains("confirmed"))),
                Times.Once);

            _mockNotifier.Verify(n =>
                n.SendAsync(It.IsAny<Guid>(), It.Is<string>(msg => msg.Contains("fully booked"))),
                Times.Once);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Update_Fails()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 2);

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(r => r.UpdateAsync(slot)).ReturnsAsync(false);

            var command = new BookSlotCommand(slot.Id);
            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to persist booking update.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }
    }
}
