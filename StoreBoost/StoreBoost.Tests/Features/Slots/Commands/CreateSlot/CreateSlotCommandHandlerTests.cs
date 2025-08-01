using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Commands.CreateSlot;
using StoreBoost.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace StoreBoost.Tests.Features.Slots.Commands.CreateSlot
{
    public class CreateSlotCommandHandlerTests
    {
        private readonly Mock<ISlotRepository> _mockRepository;
        private readonly Mock<ILogger<CreateSlotCommandHandler>> _mockLogger;
        private readonly CreateSlotCommandHandler _handler;

        public CreateSlotCommandHandlerTests()
        {
            _mockRepository = new Mock<ISlotRepository>();
            _mockLogger = new Mock<ILogger<CreateSlotCommandHandler>>();

            _handler = new CreateSlotCommandHandler(
                _mockRepository.Object,
                _mockLogger.Object
            );
        }

        [Fact]
        public async Task Should_Create_New_Slot_With_Valid_Values()
        {
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(2), 3);

            _mockRepository.Setup(r => r.IsOverlappingSlotExistsAsync(command.StartTime))
                           .ReturnsAsync(false);

            _mockRepository.Setup(r => r.AddAsync(It.IsAny<AppointmentSlot>()))
                           .Returns(Task.CompletedTask);

            var result = await _handler.Handle(command, CancellationToken.None);

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
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(1), 2);

            _mockRepository.Setup(r => r.IsOverlappingSlotExistsAsync(command.StartTime))
                           .ReturnsAsync(true);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("A slot already exists in this time window.");

            _mockRepository.Verify(r => r.AddAsync(It.IsAny<AppointmentSlot>()), Times.Never);
        }

        [Fact]
        public async Task Should_Return_Failure_If_Exception_Occurs()
        {
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(3), 1);

            _mockRepository.Setup(r => r.IsOverlappingSlotExistsAsync(command.StartTime))
                           .ThrowsAsync(new Exception("DB error"));

            var result = await _handler.Handle(command, CancellationToken.None);

            result.Success.Should().BeFalse();
            result.Message.Should().Be("An unexpected error occurred while creating the slot.");

            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString().Contains("Error occurred while creating a slot")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
