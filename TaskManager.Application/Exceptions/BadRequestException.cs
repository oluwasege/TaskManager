using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Application.Exceptions
{
    public class BadRequestException : RequestErrorException
    {
        public BadRequestException()
            : base()
        {
        }

        public BadRequestException(string message)
                : base(message)
        {
        }

        public BadRequestException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
