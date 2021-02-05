using System;

namespace Pienetes.App.Domain.Exceptions
{
    public class SecretException : Exception
    {
        public SecretException(string message) : base(message)
        {
            
        }
    }
}