using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BlazorWasmWithAADAuth.Client.Services;

namespace BlazorWasmWithAADAuth.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            //builder.Services.AddHttpClient("BlazorWasmWithAADAuth.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
            //    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();
            builder.Services.AddHttpClient<HTTPClientBackendService>("BlazorWasmWithAADAuth.ServerAPI",
                client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            builder.Services.AddTransient<GraphCustomAuthorizationMessageHandler>();
            builder.Services.AddHttpClient<GraphHTTPClientService>("BlazorWasmWithAADAuth.GraphAPI",
                client => client.BaseAddress = new Uri("https://graph.microsoft.com/"))
                .AddHttpMessageHandler<GraphCustomAuthorizationMessageHandler>();



            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorWasmWithAADAuth.ServerAPI"));
            builder.Services.AddTransient(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("BlazorWasmWithAADAuth.GraphAPI"));

            builder.Services.AddMsalAuthentication<RemoteAuthenticationState,
                CustomUserAccount>(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add("8a3cc5ba-76dc-419a-ade8-fa3534c71ae2/API.Access");
                options.UserOptions.RoleClaim = "role";
            }).AddAccountClaimsPrincipalFactory<RemoteAuthenticationState, CustomUserAccount,
            CustomUserFactory>();

            await builder.Build().RunAsync();
        }
    }
}
