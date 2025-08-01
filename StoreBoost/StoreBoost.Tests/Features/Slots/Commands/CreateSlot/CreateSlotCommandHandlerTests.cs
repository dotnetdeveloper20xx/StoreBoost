using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.CreateSlot;
using StoreBoost.Application.Interfaces;

namespace StoreBoost.Tests.Features.Slots.Commands.CreateSlot
{
    public class CreateSlotCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly CreateSlotCommandHandler _handler;

        public CreateSlotCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _handler = new CreateSlotCommandHandler(_mockRepository.Object);
        }

        [Fact]
        public async Task Should_Create_New_Slot_With_Valid_Values()
        {
            // Arrange
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(2), 3);

            _mockRepository.Setup(r => r.IsOverlappingSlotExistsAsync(command.StartTime))
                           .ReturnsAsync(false);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<AppointmentSlot>()))
                           .Returns(Task.CompletedTask);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBe(Guid.Empty);
            result.Message.Should().Be("Slot created successfully.");

            _mockRepository.Verify(r => r.AddAsync(It.Is<AppointmentSlot>(s =>
                s.StartTime == command.StartTime &&
                s.MaxBookings == command.MaxBookings
            )), Times.Once);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Overlapping_Slot_Exists()
        {
            // Arrange
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(1), 2);

            _mockRepository.Setup(r => r.IsOverlappingSlotExistsAsync(command.StartTime))
                           .ReturnsAsync(true);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.Success.Should().BeFalse();
            result.Message.Should().Be("A slot already exists in this time window.");

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<AppointmentSlot>()), Times.Never);
        }
    }
}
