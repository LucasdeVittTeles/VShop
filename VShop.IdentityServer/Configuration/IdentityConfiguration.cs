using Duende.IdentityServer.Models;

namespace VShop.IdentityServer.Configuration
{
    public class IdentityConfiguration
    {

        public const string Admin = "Admin";
        public const string Client = "Client";

        public static IEnumerable<IdentityResource> IdentityResources => new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Email(),
            new IdentityResources.Profile(),
        };

        //public static IEnumerable<IdentAityResource> ApiScopes => new List<IdentityResource>
        //{
        //    new IdentityResources.OpenId(),
        //    new IdentityResources.Email(),
        //    new IdentityResources.Profile(),
        //};


    }
}
