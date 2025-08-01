using FluentValidation.TestHelper;
using StoreBoost.Application.Features.Slots.Commands.CreateSlot;

namespace StoreBoost.Tests.Features.Slots.Commands.CreateSlot
{
    /// <summary>
    /// Unit tests for CreateSlotCommandValidator to ensure time and booking constraints are enforced.
    /// </summary>
    public class CreateSlotCommandValidatorTests
    {
        private readonly CreateSlotCommandValidator _validator;

        public CreateSlotCommandValidatorTests()
        {
            _validator = new CreateSlotCommandValidator();
        }

        [Fact]
        public void Should_Fail_When_StartTime_Is_In_The_Past()
        {
            // Arrange
            var pastTime = DateTime.UtcNow.AddMinutes(-10);
            var command = new CreateSlotCommand(pastTime, MaxBookings: 2);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.StartTime)
                  .WithErrorMessage("Start time must be in the future.");
        }

        [Fact]
        public void Should_Fail_When_MaxBookings_Is_Less_Than_One()
        {
            // Arrange
            var futureTime = DateTime.UtcNow.AddHours(1);
            var command = new CreateSlotCommand(futureTime, MaxBookings: 0);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.MaxBookings)
                  .WithErrorMessage("Max bookings must be at least 1.");
        }

        [Fact]
        public void Should_Pass_When_Command_Is_Valid()
        {
            // Arrange
            var validCommand = new CreateSlotCommand(DateTime.UtcNow.AddMinutes(30), 3);

            // Act
            var result = _validator.TestValidate(validCommand);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
            result.ShouldNotHaveValidationErrorFor(x => x.MaxBookings);
        }
    }
}
