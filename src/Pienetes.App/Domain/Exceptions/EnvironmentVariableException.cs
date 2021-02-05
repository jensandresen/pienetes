using System;

namespace Pienetes.App.Domain.Exceptions
{
    public class EnvironmentVariableException : Exception
    {
        public EnvironmentVariableException(string message) : base(message)
        {
            
        }
    }
}