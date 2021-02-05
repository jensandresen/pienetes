using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence.Converters
{
    public class ServiceIdConverter : ValueConverter<ServiceId, string>
    {
        public ServiceIdConverter() : base(ToDb, FromDb)
        {
        }

        private static Expression<Func<ServiceId, string>> ToDb => value => value.ToString();
        private static Expression<Func<string, ServiceId>> FromDb => text => ServiceId.Create(text);
    }
}