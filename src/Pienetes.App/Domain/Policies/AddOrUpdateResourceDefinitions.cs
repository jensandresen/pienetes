using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Pienetes.App.Application;
using Pienetes.App.Domain.Events;
using Pienetes.App.Infrastructure.Persistence;
using YamlDotNet.RepresentationModel;

namespace Pienetes.App.Domain.Policies
{
    public class AddOrUpdateResourceDefinitions : IEventHandler<NewManifestHasBeenQueued>
    {
        private readonly IQueuedManifestRepository _queuedManifestRepository;
        private readonly IServiceDefinitionApplicationService _serviceDefinitionApplicationService;

        public AddOrUpdateResourceDefinitions(IQueuedManifestRepository queuedManifestRepository, 
            IServiceDefinitionApplicationService serviceDefinitionApplicationService)
        {
            _queuedManifestRepository = queuedManifestRepository;
            _serviceDefinitionApplicationService = serviceDefinitionApplicationService;
        }
        
        public async Task Handle(NewManifestHasBeenQueued e)
        {
            Console.WriteLine($"hello from {this.GetType().Name}");
            // var queuedManifest = await _queuedManifestRepository.Get(e.ManifestId);
            // if (queuedManifest == null)
            // {
            //     return;
            // }
            //
            // var manifest = ManifestEmbeddedDocument.Parse(queuedManifest.Content);
            //
            // // handle all resource definition types here!
            // var serviceDefinition = manifest.GetServiceDefinition();
            // if (serviceDefinition == null)
            // {
            //     await _queuedManifestRepository.Remove(queuedManifest.Id);
            //     return;
            // }
            //
            // await _serviceDefinitionApplicationService.AddOrUpdateService(
            //     serviceName: serviceDefinition.Name,
            //     image: ServiceImage.Parse(serviceDefinition.Image),
            //     ports: serviceDefinition.Ports.Select(ServicePortMapping.Parse),
            //     secrets: serviceDefinition.Secrets.Select(ServiceSecret.Parse),
            //     environmentVariables: serviceDefinition.EnvironmentVariables.Select(x =>
            //         new ServiceEnvironmentVariable(x.Key, x.Value))
            // );
        }
    }
    
    public class ManifestEmbeddedDocument
    {
        private readonly YamlStream _yaml;

        private ManifestEmbeddedDocument(YamlStream yaml)
        {
            _yaml = yaml;
        }

        public int Version
        {
            get
            {
                var root = (YamlMappingNode)_yaml.Documents[0].RootNode;
                
                if (!root.Children.ContainsKey("version"))
                {
                    throw new ManifestException("Missing required root field \"version\".");
                }
                
                var version = (YamlScalarNode)root["version"];
                return int.Parse(version.Value ?? string.Empty);
            }
        }
        
        public ServiceDefinition GetServiceDefinition()
        {
            const string fieldName = "service";

            var root = (YamlMappingNode)_yaml.Documents[0].RootNode;
            if (!root.Children.ContainsKey(fieldName))
            {
                return null;
            }
                
            var service = (YamlMappingNode)root[fieldName];

            if (!service.Children.TryGetValue("name", out var name))
            {
                throw new ManifestException("Missing required field \"name\" in service definition.");
            }

            if (!service.Children.TryGetValue("image", out var image))
            {
                throw new ManifestException("Missing required field \"image\" in service definition.");
            }

            var ports = Array.Empty<string>();
            if (service.Children.TryGetValue("ports", out var portsNode))
            {
                ports = (((YamlSequenceNode) portsNode)
                    .Children
                    .Cast<YamlScalarNode>()?
                    .Select(x => x.Value ?? ""))
                    .ToArray();
            }

            var secrets = Array.Empty<string>();
            if (service.Children.TryGetValue("secrets", out var secretsNode))
            {
                secrets = (((YamlSequenceNode) secretsNode)
                        .Children
                        .Cast<YamlScalarNode>()?
                        .Select(x => x.Value ?? ""))
                    .ToArray();
            }

            var environmentVariables = new Dictionary<string, string>();
            if (service.Children.TryGetValue("environmentVariables", out var envNode))
            {
                environmentVariables = ((YamlMappingNode) envNode)
                    .Children
                    .Select(x =>
                    {
                        var key = ((YamlScalarNode) x.Key).Value ?? "";
                        var value = ((YamlScalarNode) x.Value).Value ?? "";
                        return new KeyValuePair<string, string>(key, value);
                    })
                    .ToDictionary(x => x.Key, x => x.Value);
            }
            
            return new ServiceDefinition(
                Name: ((YamlScalarNode)name)?.Value,
                Image: ((YamlScalarNode)image)?.Value,
                Ports: ports,
                Secrets: secrets,
                EnvironmentVariables: environmentVariables
            );
        }
        
        public record ServiceDefinition(string Name, string Image, string[] Ports, string[] Secrets,
            Dictionary<string, string> EnvironmentVariables);
        
        public static ManifestEmbeddedDocument Parse(string text)
        {
            using (var reader = new StringReader(text))
            {
                var yamlStream = new YamlStream();
                yamlStream.Load(reader);

                var manifest = new ManifestEmbeddedDocument(yamlStream);
                var version = manifest.Version;

                return manifest;
            }
        }

        public class ManifestException : Exception
        {
            public ManifestException(string  message) : base(message)
            {
                
            }
        }
    }
}