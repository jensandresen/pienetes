using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence.Converters
{
    public class ServicePortMappingConverter : ValueConverter<IEnumerable<ServicePortMapping>, string>
    {
        public ServicePortMappingConverter() : base(ToDb, FromDb)
        {
        }

        private static Expression<Func<IEnumerable<ServicePortMapping>, string>> ToDb =>
            mappings => string.Join(
                ",",
                mappings.Select(x => x.ToString())
            );

        private static Expression<Func<string, IEnumerable<ServicePortMapping>>> FromDb =>
            text => (text ?? "")
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ServicePortMapping.Parse);
    }
}