using System;
using System.Collections.Generic;
using System.Linq;
using Pienetes.App.Domain.Events;
using Pienetes.App.Domain.Exceptions;

namespace Pienetes.App.Domain.Model
{
    public class ServiceDefinition : AggregateRoot<ServiceId>
    {
        private ServiceImage _image;
        private IList<ServicePortMapping> _ports;
        private IList<ServiceSecret> _secrets;
        private IList<ServiceEnvironmentVariable> _environmentVariables;

        private ServiceDefinition(ServiceId id, ServiceImage image, IEnumerable<ServicePortMapping> ports, 
            IEnumerable<ServiceSecret> secrets, IEnumerable<ServiceEnvironmentVariable> environmentVariables) : base(id)
        {
            _image = image;
            _ports = new List<ServicePortMapping>(ports);
            _secrets = new List<ServiceSecret>(secrets);
            _environmentVariables = new List<ServiceEnvironmentVariable>(environmentVariables);
        }

        public ServiceImage Image => _image;

        public void Change(ServiceImage newImage, IEnumerable<ServicePortMapping> ports, IEnumerable<ServiceSecret> secrets, 
            IEnumerable<ServiceEnvironmentVariable> environmentVariables)
        {
            var hasChanged = false;

            if (_image != newImage)
            {
                _image = newImage;
                hasChanged = true;
            }

            var newPorts = ports.ToList();
            if (!_ports.SequenceEqual(newPorts))
            {
                _ports = newPorts;
                hasChanged = true;
            }

            var newSecrets = secrets.ToList();
            if (!_secrets.SequenceEqual(newSecrets))
            {
                _secrets = newSecrets;
                hasChanged = true;
            }

            var newEnvironmentVariables = environmentVariables.ToList();
            if (!_environmentVariables.SequenceEqual(newEnvironmentVariables))
            {
                _environmentVariables = newEnvironmentVariables;
                hasChanged = true;
            }

            if (hasChanged)
            {
                this.Raise(new ExistingServiceDefinitionHasBeenChanged(Id));
            }
        }

        public string Checksum => ChecksumHelper.ComputeChecksum(EnvironmentVariables, Secrets, Ports, Image, Id);

        public IEnumerable<ServicePortMapping> Ports => _ports;
        public void AddPortMapping(ServicePortMapping portMapping)
        {
            if (_ports.Contains(portMapping))
            {
                throw new PortMappingException($"Port mapping {portMapping} already exists.");
            }
            
            if (_ports.Any(x => x.HostPort == portMapping.HostPort))
            {
                throw new PortMappingException($"Host port of mapping {portMapping} already mapped.");
            }
            
            _ports.Add(portMapping);
        }

        public void AddPortMappings(IEnumerable<ServicePortMapping> portMappings)
        {
            foreach (var mapping in portMappings)
            {
                AddPortMapping(mapping);
            }
        }
        
        public IEnumerable<ServiceSecret> Secrets => _secrets;
        public void AddSecret(ServiceSecret secret)
        {
            if (_secrets.Contains(secret))
            {
                throw new SecretException($"Secret {secret} already added.");
            }

            if (_secrets.Any(x => x.Name.Equals(secret.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new SecretException($"Secret with name '{secret.Name}' already exists.");
            }
            
            _secrets.Add(secret);
        }

        public void AddSecrets(IEnumerable<ServiceSecret> secrets)
        {
            foreach (var secret in secrets)
            {
                AddSecret(secret);
            }
        }

        public IEnumerable<ServiceEnvironmentVariable> EnvironmentVariables => _environmentVariables;
        public void AddEnvironmentVariable(ServiceEnvironmentVariable variable)
        {
            if (_environmentVariables.Contains(variable))
            {
                throw new EnvironmentVariableException($"Environment variable {variable} already added.");
            }
            
            if (_environmentVariables.Any(x => x.Name.Equals(variable.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                throw new EnvironmentVariableException($"Environment variable with name {variable.Name} already exists.");
            }
            
            _environmentVariables.Add(variable);
        }

        public void AddEnvironmentVariables(IEnumerable<ServiceEnvironmentVariable> variables)
        {
            foreach (var variable in variables)
            {
                AddEnvironmentVariable(variable);
            }
        }
        
        public static ServiceDefinition Create(ServiceId id, ServiceImage image)
        {
            var instance =  new ServiceDefinition(
                id: id,
                image: image,
                ports: Enumerable.Empty<ServicePortMapping>(), 
                secrets: Enumerable.Empty<ServiceSecret>(), 
                environmentVariables: Enumerable.Empty<ServiceEnvironmentVariable>()
            );
            
            instance.Raise(new NewServiceDefinitionAdded(instance.Id));

            return instance;
        }
    }
}