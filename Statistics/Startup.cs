using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Statistics.EventBus;
using Statistics.RabbitMQHelpers;
using Statistics.Events;
using Statistics.EventHandlers;
using Microsoft.EntityFrameworkCore;

namespace Statistics
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
            services.AddMvc();
            services.AddDbContext<ApplicationDbContext>(ops => ops.UseInMemoryDatabase("Statistics"));
            services.AddSingleton<IRabbitMQPersistentConnection, RabbitMQPersistentConnection>();
            services.AddSingleton<IEventBus, RabbitMQEventBus>();
            services.AddSingleton<IEventHandler, AddNewsEventHandler>();
            services.AddSingleton<IEventHandler, AddUserEventHandler>();
            services.AddSingleton<IEventHandler, LoginEventHandler>();
            services.AddSingleton<IEventHandler, RequestEventHandler>();
            services.AddTransient<DbProxy>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.ApplicationServices.GetServices<IEventHandler>();
            app.UseMvc();
        }
    }
}
