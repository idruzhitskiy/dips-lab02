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
        public DbSet<Client> Clients { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<User> Users { get; set; }

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
                Clients.AddRange(
                    new Client
                    {
                        ClientId = "Gateway",
                        AllowedGrantTypes = GrantTypes.ClientCredentials,
                        AllowOfflineAccess = true,
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
            return await ApiResources.FirstOrDefaultAsync(r => r.Name == name);
        }

        public async Task<Resources> GetAllResourcesAsync()
        {
            return new Resources(IdentityResources, ApiResources);
        }

        public async Task<Client> FindClientByIdAsync(string clientId)
        {
            return await Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
        }
    }
}
