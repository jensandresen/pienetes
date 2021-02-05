using System;

namespace Pienetes.App.Infrastructure.Messaging
{
    public interface IMessageSerializer
    {
        string Serialize(object message);
        T Deserialize<T>(string message);
        object Deserialize(string message, Type resultType);
    }
}