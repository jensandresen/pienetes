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

            // services
            //     .AddEntityFrameworkNpgsql()
            //     .AddDbContext<PienetesDbContext>(options => options.UseNpgsql(connectionString));

            services.AddDbContext<PienetesDbContext>(options => options.UseNpgsql(connectionString));
            
            services.AddTransient<IServiceDefinitionRepository, ServiceDefinitionRepository>();

            services.AddTransient<OutboxManifestApplicationServiceDecorator>();
            services.AddTransient<TransactionalManifestApplicationServiceDecorator>();
            services.AddTransient<OutboxScheduleImmediatelyManifestApplicationService>();
            services.AddTransient<ManifestApplicationService>();
            services.AddTransient<IMessageSerializer, JsonMessageSerializer>();
            services.AddTransient<IQueuedManifestRepository, QueuedManifestRepository>();
            
            services.AddTransient<IManifestApplicationService>(serviceProvider =>
            {
                var inner = serviceProvider.GetRequiredService<ManifestApplicationService>();

                var layer1 = new OutboxManifestApplicationServiceDecorator(
                    inner: inner,
                    eventRegistry: serviceProvider.GetRequiredService<IEventRegistry>(),
                    serializer: serviceProvider.GetRequiredService<IMessageSerializer>(),
                    dbContext: serviceProvider.GetRequiredService<PienetesDbContext>()
                );

                var layer2 = new TransactionalManifestApplicationServiceDecorator(
                    inner: layer1,
                    dbContext: serviceProvider.GetRequiredService<PienetesDbContext>()
                );

                var layer3 = new OutboxScheduleImmediatelyManifestApplicationService(
                    inner: layer2,
                    outboxScheduler: serviceProvider.GetRequiredService<IOutboxScheduler>()
                );

                return layer3;
            });

            services.AddTransient<IServiceDefinitionApplicationService, ServiceDefinitionApplicationService>();

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
                        // handlers.Add<>()
                    }
                )
                .Register<ExistingServiceDefinitionHasBeenChanged>(
                    eventType: "existing_service_definition_has_changed",
                    topic: "pienetes.service_definitions",
                    handlers: handlers =>
                    {

                    }
                );
            
            services.AddSingleton<IEventRegistry>(eventRegistry);
            
            foreach (var handlerType in eventRegistry
                .Registrations
                .SelectMany(x => x.EventHandlerInstanceTypes))
            {
                services.AddTransient(handlerType);
            }

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
