using FluentValidation.TestHelper;
using StoreBoost.Application.Features.Slots.Commands.CreateSlot;

namespace StoreBoost.Tests.Features.Slots.Commands.CreateSlot
{
    public class CreateSlotCommandValidatorTests
    {
        private readonly CreateSlotCommandValidator _validator;

        public CreateSlotCommandValidatorTests()
        {
            _validator = new CreateSlotCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_StartTime_Is_In_The_Past()
        {
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(-1), 2);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.StartTime)
                  .WithErrorMessage("Start time must be in the future.");
        }

        [Fact]
        public void Should_Have_Error_When_MaxBookings_Is_Less_Than_One()
        {
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(1), 0);
            var result = _validator.TestValidate(command);
            result.ShouldHaveValidationErrorFor(x => x.MaxBookings)
                  .WithErrorMessage("Max bookings must be at least 1.");
        }

        [Fact]
        public void Should_Not_Have_Errors_When_Command_Is_Valid()
        {
            var command = new CreateSlotCommand(DateTime.UtcNow.AddHours(1), 3);
            var result = _validator.TestValidate(command);
            result.ShouldNotHaveValidationErrorFor(x => x.StartTime);
            result.ShouldNotHaveValidationErrorFor(x => x.MaxBookings);
        }
    }
}
