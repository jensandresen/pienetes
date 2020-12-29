using System;

namespace Pienetes.App.Domain
{
    public class EnvironmentVariableException : Exception
    {
        public EnvironmentVariableException(string message) : base(message)
        {
            
        }
    }
}