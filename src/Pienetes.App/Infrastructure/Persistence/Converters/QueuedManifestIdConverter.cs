using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence.Converters
{
    public class QueuedManifestIdConverter : ValueConverter<QueuedManifestId, string>
    {
        public QueuedManifestIdConverter() : base(ToDb, FromDb)
        {
        }

        private static Expression<Func<QueuedManifestId, string>> ToDb => value => value.ToString();
        private static Expression<Func<string, QueuedManifestId>> FromDb => text => QueuedManifestId.Parse(text);
    }
}