using System;

namespace Pienetes.App.Domain.Exceptions
{
    public class PortMappingException : Exception
    {
        public PortMappingException(string message) : base(message)
        {
            
        }
    }
}