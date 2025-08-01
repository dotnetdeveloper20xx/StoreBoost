using FluentAssertions;
using Moq;
using StoreBoost.Application.Features.Slots.Queries.GetSlots;
using StoreBoost.Application.Interfaces;
using StoreBoost.Domain.Entities;

namespace StoreBoost.Tests.Features.Slots.Queries.GetSlots
{
    public class GetSlotsQueryHandlerTests
    {
        [Fact]
        public async Task Should_Return_All_Slots()
        {
            // Arrange
            var mockRepo = new Mock<ISlotRepository>();
            var slots = new List<AppointmentSlot>
            {
                new(Guid.NewGuid(), DateTime.Today.AddHours(9)),
                new(Guid.NewGuid(), DateTime.Today.AddHours(10))
            };

            mockRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(slots);

            var handler = new GetSlotsQueryHandler(mockRepo.Object);

            // Act
            var result = await handler.Handle(new GetSlotsQuery(), CancellationToken.None);

            // Assert
            result.Success.Should().BeTrue();
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data!.All(s => s.Id != Guid.Empty).Should().BeTrue();
        }
    }
}
