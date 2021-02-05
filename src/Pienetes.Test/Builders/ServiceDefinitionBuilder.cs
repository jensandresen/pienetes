using System.Collections.Generic;
using System.Linq;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.Test.Builders
{
    public class ServiceDefinitionBuilder
    {
        private ServiceId _id;
        private ServiceImage _image;
        private IEnumerable<ServicePortMapping> _ports;
        private IEnumerable<ServiceSecret> _secrets;
        private IEnumerable<ServiceEnvironmentVariable> _environmentVariables;

        public ServiceDefinitionBuilder()
        {
            _id = ServiceId.Create("foo-id");
            _image = new ServiceImage("foo-image");
            _ports = Enumerable.Empty<ServicePortMapping>();
            _secrets = Enumerable.Empty<ServiceSecret>();
            _environmentVariables = Enumerable.Empty<ServiceEnvironmentVariable>();
        }
        
        public ServiceDefinitionBuilder WithId(string id)
        {
            _id = ServiceId.Create(id);
            return this;
        }
        
        public ServiceDefinitionBuilder WithImage(string image)
        {
            _image = new ServiceImage(image);
            return this;
        }
        
        public ServiceDefinitionBuilder WithPorts(params ServicePortMapping[] ports)
        {
            _ports = new List<ServicePortMapping>(ports);
            return this;
        }

        public ServiceDefinitionBuilder WithSecrets(params ServiceSecret[] secrets)
        {
            _secrets = new List<ServiceSecret>(secrets);
            return this;
        }

        public ServiceDefinitionBuilder WithEnvironmentVariables(params ServiceEnvironmentVariable[] variables)
        {
            _environmentVariables = new List<ServiceEnvironmentVariable>(variables);
            return this;
        }

        public ServiceDefinition Build()
        {
            var def = ServiceDefinition.Create(
                id: _id,
                image: _image
            );

            foreach (var x in _ports)
            {
                def.AddPortMapping(x);
            }

            foreach (var x in _secrets)
            {
                def.AddSecret(x);
            }
            
            foreach (var x in _environmentVariables)
            {
                def.AddEnvironmentVariable(x);
            }
            
            return def;
        }
    }
}