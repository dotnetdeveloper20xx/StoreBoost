using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoreBoost.Application.Interfaces
{
    public interface INotificationService
    {
        Task SendAsync(Guid userId, string message);
    }

}
