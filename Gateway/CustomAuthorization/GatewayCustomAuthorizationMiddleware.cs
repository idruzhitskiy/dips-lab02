﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Gateway.CustomAuthorization
{
    public class GatewayCustomAuthorizationMiddleware : CustomAuthorizationMiddleware
    {
        public GatewayCustomAuthorizationMiddleware(RequestDelegate next, TokensStore tokensStore) : base(next, tokensStore)
        {
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
