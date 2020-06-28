using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorWasmWithAADAuth.Client.Services
{
    public class GraphCustomAuthorizationMessageHandler : AuthorizationMessageHandler
    {
        public GraphCustomAuthorizationMessageHandler(IAccessTokenProvider provider,
            NavigationManager navigationManager)
            : base(provider, navigationManager)
        {
            ConfigureHandler(
                authorizedUrls: new[] { "https://graph.microsoft.com/" },
                scopes: new[] { "Application.Read.All", "Group.Read.All", "User.Read.All" });
        }
    }
}
