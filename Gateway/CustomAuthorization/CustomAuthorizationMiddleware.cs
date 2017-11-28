using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Gateway.CustomAuthorization
{
    public class CustomAuthorizationMiddleware
    {
        private const string authorizationWord = "Authorization";
        private List<(string, string)> allowedApps = new List<(string,string)> { ("AppId", "AppSecret") };
        private readonly RequestDelegate _next;
        private readonly TokensStore tokensStore;
        private List<string> anonymousPaths = new List<string>
        {
            "api",
            "login",
            "authenticate"
        };

        public CustomAuthorizationMiddleware(RequestDelegate next, TokensStore tokensStore)
        {
            _next = next;
            this.tokensStore = tokensStore;
        }

        public async Task Invoke(HttpContext context)
        {
            if (RequestedToken(context))
            {
                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(tokensStore.GetToken(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(5)));
                return;
            }

            if (!context.Request.Path.Value.Split('/').Intersect(anonymousPaths).Any())
            {
                if(context.Request.Headers.Keys.Contains(authorizationWord))
                {
                    var auth = context.Request.Headers[authorizationWord];
                    await CheckAuthorization(context, auth);
                }
                else if (context.Request.Cookies.Keys.Contains(authorizationWord))
                {
                    var auth = context.Request.Cookies[authorizationWord];
                    await CheckAuthorization(context, auth);
                }
                else
                {
                    var message = "No authorization header provided";
                    await ReturnForbidden(context, message);
                }
            }
            else
            {
                await this._next(context);
            }
        }

        private bool RequestedToken(HttpContext context)
        {
            if (context.Request.Headers.Keys.Contains("Authorization"))
            {
                var match = Regex.Match(string.Join(string.Empty, context.Request.Headers["Authorization"]), @"Basic (\S+)");
                if (match.Groups.Count > 1)
                {
                    var appIdAndSecret = Encoding.UTF8.GetString(Convert.FromBase64String(match.Groups[1].Value)).Split(':');
                    if (allowedApps.Contains((appIdAndSecret[0], appIdAndSecret[1])))
                        return true;
                }
            }
            return false;
        }

        private async Task CheckAuthorization(HttpContext context, string auth)
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
                    await this._next(context);
                else if (result == CheckTokenResult.Expired)
                    await ReturnForbidden(context, "Token has expired");
                else
                    await ReturnForbidden(context, "Token not valid");
            }
        }

        private static async Task ReturnForbidden(HttpContext context, string message)
        {
            using (var writer = new StreamWriter(context.Response.Body))
            {
                context.Response.StatusCode = 401; 
                await writer.WriteAsync(message);
            }
        }
    }
}
