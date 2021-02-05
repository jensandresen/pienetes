using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence.Converters
{
    public class ServiceImageConverter : ValueConverter<ServiceImage, string>
    {
        public ServiceImageConverter() : base(ToDb, FromDb)
        {
            
        }

        private static Expression<Func<ServiceImage, string>> ToDb => image => image.ToString();
        private static Expression<Func<string, ServiceImage>> FromDb => value => ServiceImage.Parse(value);
    }
}