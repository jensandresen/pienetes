using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Pienetes.App.Domain
{
    public class ServiceImage : ValueObject
    {
        public ServiceImage(string name, string tag = "latest")
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"Invalid value '{name}' for {nameof(name)}.");
            }
            
            if (string.IsNullOrWhiteSpace(tag))
            {
                throw new ArgumentException($"Invalid value '{tag}' for {nameof(tag)}.");
            }
            
            Name = name;
            Tag = tag;
        }
        
        public string Name { get; }
        public string Tag { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Tag;
        }
        
        public override string ToString()
        {
            return string.Join(":", Name, Tag);
        }

        public static ServiceImage Parse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException($"Invalid text ('{text}') for parsing into an image.");
            }
            
            var match = Regex.Match(text, @"^\s*(?<name>.*?)(:(?<tag>.*?))?\s*$");
            var name = match.Groups["name"];
            var tag = match.Groups["tag"];

            if (tag.Success)
            {
                return new ServiceImage(
                    name: name.Value,
                    tag: tag.Value
                );
            }
            
            return new ServiceImage(name.Value);
        }
    }
}