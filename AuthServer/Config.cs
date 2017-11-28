using AuthServer.Entities;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace AuthServer
{
    public class Config
    {
    }
    public class Clients
    {
        public static IEnumerable<Client> Get()
        {
            var secret = new Secret { Value = "mysecret".Sha512() };

            return new List<Client> {
            new Client {
                ClientId = "authorizationCodeClient2",
                ClientName = "Authorization Code Client",
                ClientSecrets = new List<Secret> { secret },
                Enabled = true,
                AllowedGrantTypes = new List<string> { "authorization_code" }, //DELTA //IdentityServer3 wanted Flow = Flows.AuthorizationCode,
                RequireConsent = true,
                AllowRememberConsent = false,
                RedirectUris =
                  new List<string> {
                       "http://gateway.loc"
                  },
                PostLogoutRedirectUris =
                  new List<string> {"http://gateway.loc"},
                AllowedScopes = new List<string> {
                    "api"
                },
                AccessTokenType = AccessTokenType.Jwt
            }
        };
        }
    }
    public static class Users
    {
        public static List<InMemoryUser> Get()
        {
            return new List<InMemoryUser> {
            new InMemoryUser {
                Subject = "1",
                Username = "user",
                Password = "pass123",
                Claims = new List<Claim> {
                    new Claim(ClaimTypes.GivenName, "GivenName"),
                    new Claim(ClaimTypes.Surname, "surname"), //DELTA //.FamilyName in IdentityServer3
                    new Claim(ClaimTypes.Email, "user@somesecurecompany.com"),
                    new Claim(ClaimTypes.Role, "Badmin")
                }
            }
        };
        }
    }

    public class InMemoryUser
    {
        public string Subject { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public List<Claim> Claims { get; set; }
    }

    public class Scopes
    {
        // scopes define the resources in your system
        public static IEnumerable<Scope> Get()
        {
            return new List<Scope> {
            new Scope
            {
                Name = "api",
                DisplayName = "api scope",
                Emphasize = false,
            }
        };
        }
    }
}
