using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Pienetes.App.Domain;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class PienetesDbContext : DbContext
    {
        public PienetesDbContext(DbContextOptions<PienetesDbContext> options) : base(options)
        {
            
        }

        public DbSet<ServiceDefinition> ServiceDefinitions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceDefinition>(cfg =>
            {
                cfg.ToTable("service_definition");
                cfg.HasKey(x => x.Id);
                cfg.Property(x => x.Id).HasConversion(new ServiceIdConverter());
                cfg.Property(x => x.Image).HasConversion(new ServiceImageConverter());
                cfg.Ignore(x => x.Checksum);
                cfg.Property(x => x.Ports).HasConversion(new ServicePortMappingConverter());
                cfg.Property(x => x.Secrets).HasConversion(new ServiceSecretConverter());
                cfg.Property(x => x.EnvironmentVariables).HasConversion(new ServiceEnvironmentVariableConverter());
            });
        }
    }

    public class ServiceIdConverter : ValueConverter<ServiceId, string>
    {
        public ServiceIdConverter() : base(ToDb, FromDb)
        {
        }

        private static Expression<Func<ServiceId, string>> ToDb => value => value.ToString();
        private static Expression<Func<string, ServiceId>> FromDb => text => ServiceId.Create(text);
    }

    public class ServiceImageConverter : ValueConverter<ServiceImage, string>
    {
        public ServiceImageConverter() : base(ToDb, FromDb)
        {
            
        }

        private static Expression<Func<ServiceImage, string>> ToDb => image => image.ToString();
        private static Expression<Func<string, ServiceImage>> FromDb => value => ServiceImage.Parse(value);
    }

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
}