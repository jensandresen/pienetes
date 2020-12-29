using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Pienetes.App.Domain;
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

            var connectionString = Configuration["TERMINAL_OPERATION_SERVICE_DATABASE_CONNECTION_STRING"];

            services
                .AddEntityFrameworkNpgsql()
                .AddDbContext<PienetesDbContext>(options => options.UseNpgsql(connectionString));

            
            
            // services.AddDbContext<BloggingContext>(options =>
            //     options.UseNpgsql(Configuration.GetConnectionString("BloggingContext")));
            
            services.AddTransient<IServiceDefinitionRepository, ServiceDefinitionRepository>();
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
