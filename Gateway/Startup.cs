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
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Gateway.Services;
using Gateway.Services.Implementations;
using Gateway.Controllers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using IdentityServer4.AccessTokenValidation;
using Gateway.CustomAuthorization;
using Microsoft.EntityFrameworkCore;
using Statistics.EventBus;
using Gateway.Models;

namespace Gateway
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

            services.AddSingleton<IAccountsService, AccountsService>();
            services.AddSingleton<ISubscriptionsService, SubscriptionsService>();
            services.AddSingleton<INewsService, NewsService>();
            services.AddSingleton<GatewayController>();

            services.AddSingleton<TokensStore>();
            services.AddLogging(lb => lb.AddConsole());
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(o =>
            {
                o.Authority = "http://localhost:58491";
                o.RequireHttpsMetadata = false;
                o.ApiName = "api";
                o.JwtValidationClockSkew = TimeSpan.FromSeconds(0);
            });

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder => builder
                    .AllowAnyMethod()
                    .AllowAnyOrigin()
                    .AllowAnyHeader());
            });
            services.AddMvc();
            services.AddSingleton<IEventBus, RabbitMQEventBus>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseCors("AllowAll");
            app.UseMiddleware<GatewayCustomAuthorizationMiddleware>();
            app.UseAuthentication();
            app.UseStaticFiles();
            app.UseMvc();
            PopulateDbs(app.ApplicationServices.GetRequiredService<GatewayController>()).Wait();
        }

        private async Task PopulateDbs(GatewayController gatewayController)
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            List<UserModel> users = new List<UserModel>
            {
                new UserModel { Username = "User1", Password = "pass1" },
                new UserModel { Username = "User2", Password = "pass2" },
                new UserModel { Username = "User3", Password = "pass3", Role = "Admin" }
            };

            List<NewsModel> news = new List<NewsModel>
            {
                new NewsModel {Author="User2", Header = "Hot news from user 2!", Body = "News body", Date = DateTime.Now - TimeSpan.FromDays(rand.Next(30))},
                new NewsModel {Author="User3", Header = "Hot news from user 3!", Body = "News body", Date = DateTime.Now - TimeSpan.FromDays(rand.Next(30))},
                new NewsModel {Author="User3", Header = "Another hot news from user 3!", Body = "News body", Date = DateTime.Now - TimeSpan.FromDays(rand.Next(30))},
                new NewsModel {Author="User1", Header = "Hot news from user 1!", Body = "News body", Date = DateTime.Now - TimeSpan.FromDays(rand.Next(30))},
                new NewsModel {Author="User2", Header = "Hot news from user 2! Second edition", Body = "News body", Date = DateTime.Now - TimeSpan.FromDays(rand.Next(30))},
            };

            List<AddSubscriptionModel> subscriptions = new List<AddSubscriptionModel>
            {
                new AddSubscriptionModel {Author = "User2", Subscriber = "User1"},
                new AddSubscriptionModel {Author="User3", Subscriber = "User1" },
                new AddSubscriptionModel {Author = "User3", Subscriber = "User2"}
            };

            foreach (var user in users)
                await gatewayController.Register(user);
            foreach (var singleNews in news)
                await gatewayController.AddNews(singleNews);
            foreach (var subscription in subscriptions)
                await gatewayController.AddSubscription(subscription);
        }
    }
}
