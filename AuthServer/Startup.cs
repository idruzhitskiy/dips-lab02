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
using Microsoft.EntityFrameworkCore;
using IdentityServer4.Validation;
using IdentityServer4.Services;

namespace AuthServer
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
            services.AddLogging(lb => lb.AddConsole());
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("Auth"));
            var dbContext = services.BuildServiceProvider().GetRequiredService<ApplicationDbContext>();
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddResourceStore<ApplicationDbContext>()
                .AddClientStore<ApplicationDbContext>()
                .AddProfileService<ResourceOwnerPasswordValidatorAndProfileService>();

            //Inject the classes we just created
            services.AddTransient<IResourceOwnerPasswordValidator, ResourceOwnerPasswordValidatorAndProfileService>();
            services.AddTransient<IProfileService, ResourceOwnerPasswordValidatorAndProfileService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseMvc();
        }
    }
}
