using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Pienetes.App.Domain.Model
{
    public class ServiceSecret : ValueObject
    {
        public ServiceSecret(string name, string mountPath)
        {
            Name = name;
            MountPath = mountPath;
        }
            
        public string Name { get; private set; }
        public string MountPath { get; private set; }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return MountPath;
        }

        public override string ToString()
        {
            var items = new[]
            {
                Name, MountPath
            };

            return string.Join(":", items.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        public static ServiceSecret Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException($"Invalid text ('{text}') for parsing into a secret.");
            }
            
            var matches = Regex.Match(text, @"^\s*(?<name>\w+):(?<mountPath>\w+)\s*$");
            var name = matches.Groups["name"];
            var mountPath = matches.Groups["mountPath"];

            if (!name.Success || !mountPath.Success)
            {
                throw new ArgumentException($"Invalid text ('{text}') for parsing into a secret.");
            }
            
            return new ServiceSecret(
                name: name.Value,
                mountPath: mountPath.Value
            );
        }
    }
}