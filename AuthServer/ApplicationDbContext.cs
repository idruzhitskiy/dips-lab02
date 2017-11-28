using AuthServer.Entities;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthServer
{
    public class ApplicationDbContext : DbContext, IResourceStore, IClientStore
    {
        public List<Client> Clients { get; set; } = new List<Client>();
        public List<ApiResource> ApiResources { get; set; } = new List<ApiResource>();
        public List<IdentityResource> IdentityResources { get; set; } = new List<IdentityResource>();
        public List<User> Users { get; set; } = new List<User>();

        private const string scopeAllowAll = "scope.allowall";

        public ApplicationDbContext() : base()
        {
            Initialize();
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> ops) : base(ops)
        {
            Initialize();
        }

        private void Initialize()
        {
            if (!ApiResources.Any())
            {
                ApiResources.Add(new ApiResource(scopeAllowAll, "API"));
            }

            if (!Clients.Any())
            {
                Clients.Add(
                    new Client
                    {
                        ClientId = "gateway",
                        ClientName = "Gateway",
                        AllowedGrantTypes = GrantTypes.Code,
                        AllowOfflineAccess = true,
                        Enabled = true,
                        RedirectUris= new List<string> { "gateway.loc" },
                        PostLogoutRedirectUris = new List<string> { "gateway.loc" },
                        ClientSecrets =
                        {
                            new Secret("secret".Sha256())
                        },
                        AllowedScopes = { scopeAllowAll, IdentityServerConstants.StandardScopes.OfflineAccess },
                        AccessTokenLifetime = 10
                    });
            }

            if (!Users.Any())
            {
                Users.Add(new User { Username = "User1", Password = "pass1" });
                Users.Add(new User { Username = "User2", Password = "pass2" });
                Users.Add(new User { Username = "User3", Password = "pass3" });
            }
        }

        public async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            return IdentityResources.Where(r => scopeNames.Contains(r.Name));
        }

        public async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            return ApiResources.Where(r => r.Scopes.Any(s => scopeNames.Contains(s.Name)));
        }

        public async Task<ApiResource> FindApiResourceAsync(string name)
        {
            return ApiResources.FirstOrDefault(r => r.Name == name);
        }

        public async Task<Resources> GetAllResourcesAsync()
        {
            return new Resources(IdentityResources, ApiResources);
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            return Clients.FirstOrDefault(c => c.ClientId == clientId);
        }
    }
}
