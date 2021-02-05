using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Pienetes.App.Domain.Model
{
    public static class ChecksumHelper
    {
        public static string ComputeChecksum(params object[] components)
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

            foreach (var component in components)
            {
                if (component is IEnumerable)
                {
                    IncludeAll((IEnumerable<object>) component);
                }
                else
                {
                    Include(component);
                }
            }

            var hash = MD5.HashData(System.Text.Encoding.UTF8.GetBytes(checksumComponents.ToString()));

            return BitConverter
                .ToString(hash)
                .Replace("-", "")
                .ToUpperInvariant();
        }
    }
}