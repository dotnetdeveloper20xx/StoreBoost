using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreBoost.Application.Exceptions
{
    public class SlotNotFoundException : Exception
    {
        public SlotNotFoundException(Guid slotId)
            : base($"Slot with ID '{slotId}' was not found.") { }
    }
}
