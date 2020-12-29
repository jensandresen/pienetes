using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Pienetes.App.Domain
{
    public class ServiceDefinition : Entity<ServiceId>
    {
        private readonly ServiceImage _image;
        private readonly IList<ServicePortMapping> _ports;
        private readonly IList<ServiceSecret> _secrets;
        private readonly IList<ServiceEnvironmentVariable> _environmentVariables;

        private ServiceDefinition(ServiceId id, ServiceImage image, IEnumerable<ServicePortMapping> ports, 
            IEnumerable<ServiceSecret> secrets, IEnumerable<ServiceEnvironmentVariable> environmentVariables) : base(id)
        {
            _image = image;
            _ports = new List<ServicePortMapping>(ports);
            _secrets = new List<ServiceSecret>(secrets);
            _environmentVariables = new List<ServiceEnvironmentVariable>(environmentVariables);
        }

        public ServiceImage Image => _image;

        public string Checksum
        {
            get
            {
                var checksumComponents = new StringBuilder();

                void Include<T>(T item)
                {
                    checksumComponents.Append(item);
                }
                
                void IncludeAll<T>(IEnumerable<T> items)
                {
                    foreach (var item in items)
                    {
                        Include(item);
                    }
                }
                
                Include(Id);
                Include(Image);
                IncludeAll(Ports);
                IncludeAll(Secrets);
                IncludeAll(EnvironmentVariables);

                var hash = MD5.HashData(System.Text.Encoding.UTF8.GetBytes(checksumComponents.ToString()));

                return BitConverter
                    .ToString(hash)
                    .Replace("-", "")
                    .ToUpperInvariant();
            }
        }
        
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

        public static ServiceDefinition Create(ServiceId id, ServiceImage image)
        {
            return new ServiceDefinition(
                id: id,
                image: image,
                ports: Enumerable.Empty<ServicePortMapping>(), 
                secrets: Enumerable.Empty<ServiceSecret>(), 
                environmentVariables: Enumerable.Empty<ServiceEnvironmentVariable>()
            );
        }
    }
}