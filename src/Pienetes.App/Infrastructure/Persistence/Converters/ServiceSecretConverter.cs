using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence.Converters
{
    public class ServiceSecretConverter : ValueConverter<IEnumerable<ServiceSecret>, string>
    {
        public ServiceSecretConverter() : base(ToDb, FromDb)
        {
            
        }

        private static Expression<Func<string, IEnumerable<ServiceSecret>>> FromDb =>
            text => (text ?? "")
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ServiceSecret.Parse);

        private static Expression<Func<IEnumerable<ServiceSecret>, string>> ToDb =>
            secrets => string.Join(
                ",",
                secrets.Select(x => x.ToString())
            );
    }
}