using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreBoost.Application.Exceptions
{
    public class SlotAlreadyBookedException : Exception
    {
        public SlotAlreadyBookedException(Guid slotId)
            : base($"Slot with ID '{slotId}' is already fully booked.") { }
    }
}
