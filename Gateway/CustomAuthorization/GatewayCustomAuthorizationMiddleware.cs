using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Statistics.EventBus;
using Statistics.Events;

namespace Gateway.CustomAuthorization
{
    public class GatewayCustomAuthorizationMiddleware : CustomAuthorizationMiddleware
    {
        public GatewayCustomAuthorizationMiddleware(RequestDelegate next, TokensStore tokensStore, IEventBus eventBus) : base(next, tokensStore, eventBus)
        {
        }

        public override async Task Invoke(HttpContext context)
        {
            eventBus.Publish(new RequestEvent
            {
                Host = context.Connection.LocalIpAddress.ToString() + ":" + context.Connection.LocalPort.ToString(),
                Origin = context.Connection.RemoteIpAddress.ToString() + ":" + context.Connection.RemotePort.ToString(),
                Route = context.Request.Path.ToString(),
                Type = "Gateway",
                OccurenceTime = DateTime.Now
            });
            if (context.Request.Headers.Keys.Contains(AuthorizationWord))
            {
                await this._next(context);
            }
            else
                await base.Invoke(context);
        }

        public override List<string> GetAnonymousPaths() => new[] { "api", "auth", "login", "register" }.ToList();

        public override async Task ReturnForbidden(HttpContext context, string message)
        {
            string redirect = "/users/auth";
            if (context.Request.Path != redirect)
            {
                redirect += $"?Redirect={context.Request.Path}";
            }
            context.Response.Redirect(redirect);
        }
    }
}
