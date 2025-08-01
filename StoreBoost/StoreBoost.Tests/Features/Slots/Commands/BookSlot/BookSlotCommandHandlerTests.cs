using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;
using StoreBoost.Application.Interfaces;
using StoreBoost.Domain.Entities;


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
            // Create a mock repository to isolate handler logic from real data storage
            _mockRepository = new Mock<ISlotRepository>();

            // Inject the mocked repository into the handler
            _handler = new BookSlotCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Not_Found()
        {
            // Arrange
            // Simulate scenario where no slot exists for given ID
            _mockRepository.Setup(repo => repo.GetByIdAsync(It.IsAny<Guid>()))
                           .ReturnsAsync((AppointmentSlot?)null);

            var command = new BookSlotCommand(Guid.NewGuid());

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            // Expect failure with appropriate message
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot not found.");
        }

        [Fact]
        public async Task Should_Return_Failure_If_Slot_Already_Booked()
        {
            // Arrange
            // Create a slot and pre-book it
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1));
            slot.Book();

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id))
                           .ReturnsAsync(slot);

            var command = new BookSlotCommand(slot.Id);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            // Should return a failure because the slot is already booked
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Slot is already booked.");
        }

        [Fact]
        public async Task Should_Return_Success_When_Booking_Valid_Slot()
        {
            // Arrange
            // Create an available slot
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1));

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id))
                           .ReturnsAsync(slot);

            _mockRepository.Setup(repo => repo.UpdateAsync(It.IsAny<AppointmentSlot>()))
                           .ReturnsAsync(true);

            var command = new BookSlotCommand(slot.Id);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            // Expect success and confirmation message
            result.Success.Should().BeTrue();
            result.Message.Should().Be("Slot booked successfully.");
            result.Data.Should().BeTrue();
        }

        [Fact]
        public async Task Should_Return_Failure_If_Update_Fails()
        {
            // Arrange
            // Create a valid, available slot
            var slot = new AppointmentSlot(Guid.NewGuid(), DateTime.UtcNow.AddHours(1));

            _mockRepository.Setup(repo => repo.GetByIdAsync(slot.Id))
                           .ReturnsAsync(slot);

            // Simulate update failure (e.g. DB issue, concurrency)
            _mockRepository.Setup(repo => repo.UpdateAsync(slot))
                           .ReturnsAsync(false);

            var command = new BookSlotCommand(slot.Id);

            // Act
            var result = await _handler.Handle(command, default);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Failed to persist booking update.");
        }
    }
}
