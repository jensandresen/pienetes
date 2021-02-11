using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Pienetes.App.Application;
using Pienetes.App.Domain.Model;
using Pienetes.App.Infrastructure.Messaging;
using Pienetes.App.Infrastructure.Persistence.Converters;

namespace Pienetes.App.Infrastructure.Persistence
{
    public class PienetesDbContext : DbContext
    {
        public PienetesDbContext(DbContextOptions<PienetesDbContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }

        public DbSet<ServiceDefinition> ServiceDefinitions { get; set; }
        public DbSet<QueuedManifest> QueuedManifests { get; set; }
        public DbSet<OutboxMessage> OutboxMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ServiceDefinition>(cfg =>
            {
                cfg.ToTable("service_definition");
                cfg.HasKey(x => x.Id);
                cfg.Property(x => x.Id).HasConversion(new ServiceIdConverter());
                cfg.Property(x => x.Image).HasConversion(new ServiceImageConverter());
                cfg
                    .Property(x => x.Ports)
                    .HasConversion(new ServicePortMappingConverter(), new ValueObjectCollectionComparer<ServicePortMapping>());
                cfg
                    .Property(x => x.Secrets)
                    .HasConversion(new ServiceSecretConverter(), new ValueObjectCollectionComparer<ServiceSecret>());
                cfg
                    .Property(x => x.EnvironmentVariables)
                    .HasConversion(new ServiceEnvironmentVariableConverter(), new ValueObjectCollectionComparer<ServiceEnvironmentVariable>());
                cfg.Ignore(x => x.Checksum);
            });

            modelBuilder.Entity<QueuedManifest>(cfg =>
            {
                cfg.ToTable("queued_manifest");
                cfg.HasKey(x => x.Id);
                cfg.Property(x => x.Id).HasConversion(new QueuedManifestIdConverter());
                cfg.Property(x => x.Content);
                cfg.Property(x => x.ContentType);
                cfg.Property(x => x.IsProcessed);
                cfg.Property(x => x.QueuedTimestamp);
            });

            modelBuilder.Entity<OutboxMessage>(cfg =>
            {
                cfg.ToTable("outbox");
                cfg.HasKey(x => x.MessageId);
                cfg.Property(x => x.MessageType);
                cfg.Property(x => x.AggregateId);
                cfg.Property(x => x.CustomHeaders);
                cfg.Property(x => x.Payload);
                cfg.Property(x => x.CreatedAt);
                cfg.Property(x => x.SentAt);
            });
        }
    }
}