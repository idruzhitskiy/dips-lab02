﻿using IdentityModel;
using Microsoft.AspNetCore.Http;
using Statistics.EventBus;
using Statistics.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gateway.CustomAuthorization
{
    public abstract class CustomAuthorizationMiddleware
    {
        public static string AuthorizationWord = "Authorization";
        public static string UserWord = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
        public static string RoleWord = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
        protected readonly RequestDelegate _next;
        protected readonly TokensStore tokensStore;

        public CustomAuthorizationMiddleware(RequestDelegate next, TokensStore tokensStore)
        {
            _next = next;
            this.tokensStore = tokensStore;
        }

        public virtual async Task Invoke(HttpContext context)
        {
            if (context.Request.Cookies.Keys.Contains(AuthorizationWord))
            {
                var auth = context.Request.Cookies[AuthorizationWord];
                await CheckAuthorization(context, auth);
            }
            else if (context.Request.Path.Value.Split('/').Intersect(GetAnonymousPaths()).Any())
            {
                await this._next(context);
            }
            else
            {
                var message = "No authorization header provided";
                await ReturnForbidden(context, message);
            }
        }


        protected async Task CheckAuthorization(HttpContext context, string auth)
        {
            var match = Regex.Match(auth, @"Bearer (\S+)");
            if (match.Groups.Count == 1)
            {
                await ReturnForbidden(context, "Invalid token format");
            }
            else
            {
                var token = match.Groups[1].Value;
                var result = tokensStore.CheckToken(token);
                if (result == CheckTokenResult.Valid)
                {
                    ClaimsIdentity identity = new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(UserWord, tokensStore.GetNameByToken(token)),
                        new Claim(RoleWord, tokensStore.GetRoleByToken(token))
                    }, "CustomAuthenticationType");
                    context.User.AddIdentity(identity);
                    await this._next(context);
                }
                else if (result == CheckTokenResult.Expired)
                    await ReturnForbidden(context, "Token has expired");
                else
                    await ReturnForbidden(context, "Token not valid");
            }
        }

        public abstract Task ReturnForbidden(HttpContext context, string message);

        public abstract List<string> GetAnonymousPaths();
    }
}
