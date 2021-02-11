using System.Collections.Generic;
using System.Threading.Tasks;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Domain.Services
{
    public interface ICommandExecutor
    {
        Task Execute(string command);
    }

    public interface IContainerDomainService
    {
        Task PullContainerImage(ServiceImage image);
        Task RemoveContainer(ServiceId serviceId);
        Task CreateNewContainerFrom(ServiceDefinition serviceDefinition);
        Task StartContainer(ServiceId serviceId);
        Task RenameContainer(ServiceId serviceId, string newContainerName);
        Task StopContainer(string containerName);
    }

    public class ContainerDomainService : IContainerDomainService
    {
        private readonly ICommandExecutor _commandExecutor;

        public ContainerDomainService(ICommandExecutor commandExecutor)
        {
            _commandExecutor = commandExecutor;
        }

        public async Task PullContainerImage(ServiceImage image)
        {
            await _commandExecutor.Execute($"docker pull {image}");
        }

        public async Task RemoveContainer(ServiceId serviceId)
        {
            await _commandExecutor.Execute($"docker container rm {serviceId}");
        }

        public async Task CreateNewContainerFrom(ServiceDefinition serviceDefinition)
        {
            var args = new List<string>
            {
                "docker",
                "create",
                "--restart unless-stopped",
                "--network pienetes-net",
                $"--name {serviceDefinition.Id}",
            };
            
            // port mappings
            foreach (var portMap in serviceDefinition.Ports)
            {
                args.Add($"-p {portMap.HostPort}:{portMap.ContainerPort}");
            }
            
            // environment variables
            foreach (var mapping in serviceDefinition.EnvironmentVariables)
            {
                args.Add($"-e {mapping.Name}=\"{mapping.Value}\"");
            }
            
            // secrets
            // foreach (var secret in serviceDefinition.Secrets)
            // {
            //     args.Add($"-v {secret. hostFile}:{containerFile}");
            // }
            
            // **** this MUST be the last one ****
            args.Add(serviceDefinition.Image.ToString());

            var cmd = string.Join(" ", args);
            await _commandExecutor.Execute(cmd);
        }

        public async Task StartContainer(ServiceId serviceId)
        {
            await _commandExecutor.Execute($"docker start {serviceId}");
        }

        public async Task RenameContainer(ServiceId serviceId, string newContainerName)
        {
            await _commandExecutor.Execute($"docker rename {serviceId} {newContainerName}");
        }

        public async Task StopContainer(string containerName)
        {
            await _commandExecutor.Execute($"docker stop {containerName}");
        }
    }
}