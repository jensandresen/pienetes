using System.Collections.Generic;

namespace Pienetes.App.Domain.Model
{
    public class ServiceId : ValueObject
    {
        private readonly string _value;

        private ServiceId(string value)
        {
            _value = value;
        }

        public override string ToString()
        {
            return _value;
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return _value;
        }
        
        public static ServiceId Create(string serviceName)
        {
            return new ServiceId(serviceName);
        }
        
        public static implicit operator string(ServiceId serviceId)
        {
            return serviceId._value;
        }

        public static implicit operator ServiceId(string serviceName)
        {
            return new ServiceId(serviceName);
        }
    }
}