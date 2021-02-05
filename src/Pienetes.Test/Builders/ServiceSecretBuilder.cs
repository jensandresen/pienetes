using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.Test.Builders
{
    public class ServiceSecretBuilder
    {
        private string _name;
        private string _mountPath;

        public ServiceSecretBuilder()
        {
            _name = "foo";
            _mountPath = "bar";
        }

        public ServiceSecretBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public ServiceSecretBuilder WithMountPath(string mountPath)
        {
            _mountPath = mountPath;
            return this;
        }
        
        public ServiceSecret Build()
        {
            return new ServiceSecret(_name, _mountPath);
        }
    }
}