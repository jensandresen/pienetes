using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pienetes.App.Domain
{
    public class ServiceEnvironmentVariable : ValueObject
    {
        public ServiceEnvironmentVariable(string name, string value)
        {
            Name = name;
            Value = value;
        }
        
        public string Name { get; }
        public string Value { get; }
        
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Value;
        }

        public override string ToString()
        {
            return string.Join("=", Name, Value);
        }

        public static ServiceEnvironmentVariable Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException($"Invalid text ('{text}') for parsing into a environment variable.");
            }
            
            var matches = Regex.Match(text, @"^\s*(?<name>\w+)=(?<value>\w+)\s*$");
            var name = matches.Groups["name"];
            var value = matches.Groups["value"];

            if (!name.Success || !value.Success)
            {
                throw new ArgumentException($"Invalid text ('{text}') for parsing into a environment variable.");
            }
            
            return new ServiceEnvironmentVariable(
                name: name.Value,
                value: value.Value
            );
        }
    }
}