using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Pienetes.App.Application;
using Pienetes.App.Domain.Events;
using Pienetes.App.Domain.Model;
using Pienetes.App.Domain.Policies;
using Pienetes.App.Domain.Services;
using Pienetes.App.Infrastructure.CommandLine;
using Pienetes.App.Infrastructure.Messaging;
using Pienetes.App.Infrastructure.Persistence;

namespace Pienetes.App
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pienetes.App", Version = "v1" });
            });

            var connectionString = Configuration["PIENETES_DATABASE_CONNECTION_STRING"];

            services.AddDbContext<PienetesDbContext>(options => options.UseNpgsql(connectionString));
            services.AddTransient<TransactionalHelper>();
            services.AddTransient<IServiceDefinitionRepository, ServiceDefinitionRepository>();
            services.AddTransient<OutboxManifestApplicationServiceDecorator>();
            services.AddTransient<TransactionalManifestApplicationServiceDecorator>();
            services.AddTransient<OutboxScheduleImmediatelyManifestApplicationServiceDecorator>();
            services.AddTransient<IMessageSerializer, JsonMessageSerializer>();
            services.AddTransient<IQueuedManifestRepository, QueuedManifestRepository>();
            
            services.AddTransient<ManifestApplicationService>();
            services.AddTransient<IManifestApplicationService>(serviceProvider =>
            {
                var inner = serviceProvider.GetRequiredService<ManifestApplicationService>();

                var layer1 = new OutboxManifestApplicationServiceDecorator(
                    inner: inner,
                    outboxHelper: serviceProvider.GetRequiredService<OutboxHelper>()
                );

                var layer2 = new TransactionalManifestApplicationServiceDecorator(
                    inner: layer1,
                    transactionalHelper: serviceProvider.GetRequiredService<TransactionalHelper>()
                );

                var layer3 = new OutboxScheduleImmediatelyManifestApplicationServiceDecorator(
                    inner: layer2,
                    outboxScheduler: serviceProvider.GetRequiredService<IOutboxScheduler>()
                );

                return layer3;
            });

            services.AddTransient<ServiceDefinitionApplicationService>();
            services.AddTransient<IServiceDefinitionApplicationService>(serviceProvider =>
            {
                var inner = serviceProvider.GetRequiredService<ServiceDefinitionApplicationService>();

                var layer1 = new OutboxServiceDefinitionApplicationServiceDecorator(
                    inner: inner,
                    outboxHelper: serviceProvider.GetRequiredService<OutboxHelper>()
                );

                var layer2 = new TransactionalServiceDefinitionApplicationServiceDecorator(
                    inner: layer1,
                    transactionalHelper: serviceProvider.GetRequiredService<TransactionalHelper>()
                );

                var layer3 = new OutboxScheduleImmediatelyServiceDefinitionApplicationServiceDecorator(
                    inner: layer2,
                    outboxScheduler: serviceProvider.GetRequiredService<IOutboxScheduler>()
                );

                return layer3;
            });

            services.AddTransient<IContainerApplicationService, ContainerApplicationService>();
            services.AddTransient<IContainerDomainService, ContainerDomainService>();
            services.AddTransient<ICommandExecutor, CommandExecutor>();
            
            ConfigureMessaging(services);
        }

        private void ConfigureMessaging(IServiceCollection services)
        {
            var eventRegistry = new EventRegistry();

            eventRegistry
                .Register<NewManifestHasBeenQueued>(
                    eventType: "new_manifest_has_been_queued",
                    topic: "pienetes.manifests",
                    handlers: handlers =>
                    {
                        handlers.Add<AddOrUpdateResourceDefinitions>();
                    }
                )
                .Register<NewServiceDefinitionAdded>(
                    eventType: "new_service_definition_added",
                    topic: "pienetes.service_definitions",
                    handlers: handlers =>
                    {
                        handlers.Add<SpinUpContainerForService>();
                    }
                )
                .Register<ExistingServiceDefinitionHasBeenChanged>(
                    eventType: "existing_service_definition_has_changed",
                    topic: "pienetes.service_definitions",
                    handlers: handlers =>
                    {
                        handlers.Add<SpinUpContainerForService>();
                    }
                );
            
            services.AddSingleton<IEventRegistry>(eventRegistry);
            
            foreach (var handlerType in eventRegistry
                .Registrations
                .SelectMany(x => x.EventHandlerInstanceTypes))
            {
                services.AddTransient(handlerType);
            }

            services.AddTransient<OutboxHelper>();
            services.AddSingleton<OutboxDispatcher>();
            services.AddSingleton<OutboxScheduler>();
            services.AddSingleton<IOutboxScheduler>(provider => provider.GetRequiredService<OutboxScheduler>());
            services.AddSingleton<IHostedService>(provider => provider.GetRequiredService<OutboxScheduler>());

            services.AddHostedService<InboxScheduler>();
            services.AddTransient<InboxDispatcher>();
            services.AddSingleton<MessagingGateway>(_ =>
            {
                var topics = eventRegistry.Topics.ToArray();
                return MessagingGateway.Init(topics);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Pienetes.App v1"));
            }

            app.UseRouting();
            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
