using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain;
using Pienetes.App.Domain.Model;

namespace Pienetes.App.Infrastructure.Persistence.Converters
{
    public class ServiceEnvironmentVariableConverter : ValueConverter<IEnumerable<ServiceEnvironmentVariable>, string>
    {
        public ServiceEnvironmentVariableConverter() : base(ToDb, FromDb)
        {
            
        }

        private static Expression<Func<IEnumerable<ServiceEnvironmentVariable>, string>> ToDb =>
            variables => string.Join(
                ",",
                variables.Select(x => x.ToString())
            );

        private static Expression<Func<string, IEnumerable<ServiceEnvironmentVariable>>> FromDb => 
            text => (text ?? "")
                .Split(",", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(ServiceEnvironmentVariable.Parse);
    }
    
    public class ValueObjectCollectionComparer<TValueObject> : ValueComparer<IEnumerable<TValueObject>>
        where TValueObject : ValueObject
    {
        public ValueObjectCollectionComparer() : base(MyEqualsExpression, MyHashCodeExpression, MySnapshotExpression)
        {
            
        }

        private static Expression<Func<IEnumerable<TValueObject>, IEnumerable<TValueObject>>> MySnapshotExpression =>
            c => c != null ? c.ToArray() : Enumerable.Empty<TValueObject>();

        private static Expression<Func<IEnumerable<TValueObject>, int>> MyHashCodeExpression =>
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode()));

        private static Expression<Func<IEnumerable<TValueObject>, IEnumerable<TValueObject>, bool>> MyEqualsExpression => 
            (c1, c2) => c1.SequenceEqual(c2);
    }
}