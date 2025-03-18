using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Infrastructure.Messaging
{
    public class TaskCreatedMessage
    {
        public Guid TaskId { get; set; }
        public bool IsNew { get; set; }
    }
}
