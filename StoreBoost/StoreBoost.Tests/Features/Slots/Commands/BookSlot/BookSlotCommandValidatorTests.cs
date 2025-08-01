using FluentValidation.TestHelper;
using StoreBoost.Application.Features.Slots.Commands.BookSlot;

namespace StoreBoost.Tests.Features.Slots.Commands.BookSlot
{
    public class BookSlotCommandValidatorTests
    {
        private readonly BookSlotCommandValidator _validator;

        public BookSlotCommandValidatorTests()
        {
            _validator = new BookSlotCommandValidator();
        }

        [Fact]
        public void Should_Have_Error_When_SlotId_Is_Empty()
        {
            // Arrange
            var command = new BookSlotCommand(Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.SlotId)
                  .WithErrorMessage("Slot ID cannot be empty.");
        }

        [Fact]
        public void Should_Not_Have_Error_When_SlotId_Is_Valid()
        {
            // Arrange
            var command = new BookSlotCommand(Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.SlotId);
        }
    }
}
