using System;

namespace Pienetes.App.Domain
{
    public class SecretException : Exception
    {
        public SecretException(string message) : base(message)
        {
            
        }
    }
}