using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Infrastructure.Messaging
{
    public interface IMessageProducer
    {
        Task PublishTaskCreated(TaskCreatedMessage message);
    }
}
