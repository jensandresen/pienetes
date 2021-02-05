using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pienetes.App.Domain.Model
{
    public class ServicePortMapping : ValueObject
    {
        public ServicePortMapping(int hostPort, int containerPort)
        {
            HostPort = hostPort;
            ContainerPort = containerPort;
        }
            
        public int HostPort { get; }
        public int ContainerPort { get; }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return HostPort;
            yield return ContainerPort;
        }

        public override string ToString()
        {
            return string.Join(":", HostPort, ContainerPort);
        }

        public static ServicePortMapping Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Invalid value for parsing a port mapping.", nameof(text));
            }

            var match = Regex.Match(text, @"^\s*(?<host>\d+):(?<container>\d+)\s*$");
            var hostMatch = match.Groups["host"];
            var containerMatch = match.Groups["container"];

            if (!hostMatch.Success || !containerMatch.Success)
            {
                throw new ArgumentException($"Invalid value '{text}' for parsing a port mapping.", nameof(text));
            }
            
            return new ServicePortMapping(
                int.Parse(hostMatch.Value),
                int.Parse(containerMatch.Value)
            );
        }
    }
}