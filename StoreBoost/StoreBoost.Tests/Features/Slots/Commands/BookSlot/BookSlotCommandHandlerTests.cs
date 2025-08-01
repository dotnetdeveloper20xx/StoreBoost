using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;
using StoreBoost.Application.Interfaces;
using StoreBoost.Application.Exceptions;


namespace StoreBoost.Tests.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Unit tests for BookSlotCommandHandler with notification and booking logic.
    /// </summary>
    public class BookSlotCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly Mock<INotificationService> _mockNotifier;
        private readonly Mock<ILogger<BookSlotCommandHandler>> _mockLogger;
        private readonly BookSlotCommandHandler _handler;

        public BookSlotCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _mockNotifier = new Mock<INotificationService>();
            _mockLogger = new Mock<ILogger<BookSlotCommandHandler>>();

            _handler = new BookSlotCommandHandler(
                _mockRepository.Object,
                _mockNotifier.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Should_Throw_SlotNotFoundException_When_Slot_Is_Null()
        {
            // Arrange
            var command = new BookSlotCommand(Guid.NewGuid());
            _mockRepository.Setup(r => r.GetByIdAsync(command.SlotId)).ReturnsAsync((AppointmentSlot?)null);

            // Act
            var act = async () => await _handler.Handle(command, default);

            // Assert
            await act.Should().ThrowAsync<SlotNotFoundException>()
                .WithMessage($"Slot with ID '{command.SlotId}' was not found.");

            _mockNotifier.Verify(n => n.SendAsync(It.IsAny<Guid>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task Should_Throw_SlotAlreadyBookedException_When_Slot_Is_Full()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), 2);
            slot.Book(); slot.Book();

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);

            var command = new BookSlotCommand(slot.Id);

            var act = async () => await _handler.Handle(command, default);

            await act.Should().ThrowAsync<SlotAlreadyBookedException>()
                .WithMessage($"Slot with ID '{slot.Id}' is already fully booked.");

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
