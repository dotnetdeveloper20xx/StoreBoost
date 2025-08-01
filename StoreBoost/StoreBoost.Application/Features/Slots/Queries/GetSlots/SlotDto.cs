using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreBoost.Application.Features.Slots.Queries.GetSlots
{
    /// <summary>
    /// Lightweight representation of an appointment slot for API output.
    /// </summary>
    public sealed class SlotDto
    {
        public Guid Id { get; init; }
        public DateTime StartTime { get; init; }
        public int MaxBookings { get; init; }
        public int CurrentBookings { get; init; }
        public bool IsBooked { get; init; }
    }
}
