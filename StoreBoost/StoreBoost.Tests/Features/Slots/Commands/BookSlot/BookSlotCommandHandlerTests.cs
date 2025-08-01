using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;
using StoreBoost.Application.Interfaces;


namespace StoreBoost.Tests.Features.Slots.Commands.BookSlot
{
    /// <summary>
    /// Unit tests for BookSlotCommandHandler, which handles booking logic via ISlotRepository.
    /// </summary>
    public class BookSlotCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly BookSlotCommandHandler _handler;

        public BookSlotCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _handler = new BookSlotCommandHandler(_mockRepository.Object);
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
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Is_Fully_Booked()
        {
            // Arrange: Slot already at max capacity
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);
            slot.Book();
            slot.Book(); // Booked to capacity

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);

            var command = new BookSlotCommand(slot.Id);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot is fully booked.");
        }

        [Fact]
        public async Task Should_Return_Success_When_Booking_Slot_That_Is_Not_Fully_Booked()
        {
            // Arrange: Slot has remaining capacity
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 3);
            slot.Book(); // 1/3 booked

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AppointmentSlot>())).ReturnsAsync(true);

            var command = new BookSlotCommand(slot.Id);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Slot booked successfully.");
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Return_Failure_If_Update_Fails_After_Booking()
        {
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(r => r.UpdateAsync(slot)).ReturnsAsync(false);

            var command = new BookSlotCommand(slot.Id);

            var result = await _handler.Handle(command, default);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to persist booking update.");
        }

        [Fact]
        public async Task Should_Book_Final_Spot_And_Still_Succeed()
        {
            // Arrange: 1 spot left
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1), maxBookings: 2);
            slot.Book(); // 1/2 booked

            _mockRepository.Setup(r => r.GetByIdAsync(slot.Id)).ReturnsAsync(slot);
            _mockRepository.Setup(r => r.UpdateAsync(It.IsAny<AppointmentSlot>())).ReturnsAsync(true);

            var command = new BookSlotCommand(slot.Id);
            var result = await _handler.Handle(command, default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeTrue();
            slot.CurrentBookings.Should().Be(2);
            slot.IsBooked.Should().BeTrue(); // Should now be fully booked
        }
    }
}
