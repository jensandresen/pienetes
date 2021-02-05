using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.Test.Builders
{
    public class ServiceEnvironmentVariableBuilder
    {
        private string _name;
        private string _value;

        public ServiceEnvironmentVariableBuilder()
        {
            _name = "foo";
            _value = "bar";
        }

        public ServiceEnvironmentVariableBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ServiceEnvironmentVariableBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }
        
        public ServiceEnvironmentVariable Build()
        {
            return new ServiceEnvironmentVariable(_name, _value);
        }
    }
}