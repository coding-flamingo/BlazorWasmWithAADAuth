using BlazorWasmWithAADAuth.Shared.models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlazorWasmWithAADAuth.Client.Services
{
    public class GraphHTTPClientService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        private readonly ILogger _logger;
        public GraphHTTPClientService(HttpClient httpClient, ILogger<GraphHTTPClientService> logger)
        {
            _logger = logger;
            _httpClient = httpClient;
            HttpStatusCode[] httpStatusCodesWorthRetrying = {
               HttpStatusCode.RequestTimeout, // 408
               HttpStatusCode.InternalServerError, // 500
               HttpStatusCode.BadGateway, // 502
               HttpStatusCode.ServiceUnavailable, // 503
               HttpStatusCode.GatewayTimeout // 504
            };
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .OrInner<TaskCanceledException>()
                .OrResult<HttpResponseMessage>(r => httpStatusCodesWorthRetrying.Contains(r.StatusCode))
                  .WaitAndRetryAsync(new[]
                  {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(4),
                    TimeSpan.FromSeconds(8)
                  });
        }

        public async Task<List<AADObjectModel>> VaidateAADUserGroupObjectAsync(string aadObjectString)
        {
            List<AADObjectModel> aADObjects = new List<AADObjectModel>();
            if (string.IsNullOrWhiteSpace(aadObjectString))
            {
                return aADObjects;
            }
            Task<List<AADObjectModel>> groupEqResults = ValidateGroupAsync(aadObjectString);
            AADObjectModel userResult = await ValidateUserAsync(aadObjectString);
            if (userResult.isValid)
            {
                aADObjects.Add(userResult);
            }
            aADObjects.AddRange(await groupEqResults);
            if (aADObjects.Count == 0)
            {
                Task<List<AADObjectModel>> userResults = ValidateUser2ChanceAsync(aadObjectString);
                Task<List<AADObjectModel>> groupResults = ValidateGroup2ChanceAsync(aadObjectString);
                aADObjects.AddRange(await userResults);
                aADObjects.AddRange(await groupResults);
            }
            return aADObjects;
        }

        private async Task<AADObjectModel> ValidateUserAsync(string aadObjectString)
        {
            AADObjectModel aADObject = new AADObjectModel();
            UserGraphModel userResult;
            if (string.IsNullOrWhiteSpace(aadObjectString))
            {
                return new AADObjectModel() { isValid = false };
            }
            try
            {
                string response = await CallGetApiAsync("https://graph.microsoft.com/v1.0/users/" + aadObjectString + "?$select=userPrincipalName,id");
                if (response.Equals("NotFound"))
                {
                    return new AADObjectModel() { isValid = false };
                }
                else
                {
                    userResult = JsonConvert.DeserializeObject<UserGraphModel>(response);
                }
                aADObject.FriendlyName = userResult.UserPrincipalName;
                aADObject.ObjectId = userResult.Id;
                aADObject.ObjectType = "USER";
                aADObject.isValid = true;
            }
            catch (Exception ex)
            {
                //swallow exception since it can only be that it is not the right format
                aADObject.isValid = false;
            }
            return aADObject;
        }

        private async Task<List<AADObjectModel>> ValidateUser2ChanceAsync(string aadObjectString)
        {
            List<AADObjectModel> aADObjects = new List<AADObjectModel>();
            List<UserGraphModel> userResults = new List<UserGraphModel>();
            if (string.IsNullOrWhiteSpace(aadObjectString))
            {
                return aADObjects;
            }
            try
            {

                string response = await CallGetApiAsync("https://graph.microsoft.com/v1.0/users?$filter=startswith(userPrincipalName,'" + aadObjectString + "')&$select=userPrincipalName,id");
                if (response.Equals("NotFound"))
                {
                    return aADObjects;
                }
                else
                {
                    userResults = JsonConvert.DeserializeObject<UserListGraphModel>(response).value;
                }
                foreach (UserGraphModel userResult in userResults)
                {
                    aADObjects.Add(new AADObjectModel(userResult));
                }
            }
            catch (Exception ex)
            {
                //swallow exception since it can only be that it is not the right format

            }
            return aADObjects;
        }

        private async Task<List<AADObjectModel>> ValidateGroupAsync(string aadObjectString)
        {
            List<AADObjectModel> aADObjects = new List<AADObjectModel>();
            List<GroupGraphModel> groupResults;
            if (string.IsNullOrWhiteSpace(aadObjectString))
            {
                return aADObjects;
            }
            try
            {
                if (Guid.TryParse(aadObjectString, out Guid x))
                {
                    string response = await CallGetApiAsync("https://graph.microsoft.com/v1.0/groups/" + aadObjectString + "'?$select=displayName,id");
                    if (response.Equals("NotFound"))
                    {
                        return aADObjects;
                    }
                    GroupGraphModel groupResult = JsonConvert.DeserializeObject<GroupGraphModel>(response);
                    aADObjects.Add(new AADObjectModel(groupResult));
                }
                else
                {
                    string response = await CallGetApiAsync("https://graph.microsoft.com/v1.0/groups?$filter=displayName eq '" + aadObjectString + "'&$select=displayName,id");
                    if (response.Equals("NotFound"))
                    {
                        return aADObjects;
                    }
                    groupResults = JsonConvert.DeserializeObject<GroupListGraphModel>(response).value;
                    foreach (GroupGraphModel groupResult in groupResults)
                    {
                        aADObjects.Add(new AADObjectModel(groupResult));
                    }
                }

            }
            catch (Exception ex)
            {
                //swallow exception since it can only be that it is not the right format
            }
            return aADObjects;
        }

        private async Task<List<AADObjectModel>> ValidateGroup2ChanceAsync(string aadObjectString)
        {
            List<AADObjectModel> aADObjects = new List<AADObjectModel>();
            List<GroupGraphModel> groupResults;
            if (string.IsNullOrWhiteSpace(aadObjectString))
            {
                return aADObjects;
            }
            try
            {
                string response = await CallGetApiAsync("https://graph.microsoft.com/v1.0/groups?$filter=startswith(displayName,'" + aadObjectString + "')&$select=displayName,id");
                if (response.Equals("NotFound"))
                {
                    return aADObjects;
                }
                else
                {
                    groupResults = JsonConvert.DeserializeObject<GroupListGraphModel>(response).value;
                }
                foreach (GroupGraphModel groupResult in groupResults)
                {
                    aADObjects.Add(new AADObjectModel(groupResult));
                }
            }
            catch (Exception ex)
            {
                //swallow exception since it can only be that it is not the right format
            }
            return aADObjects;
        }

        private async Task<string> CallGetApiAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url is empty or null", nameof(url));
            }
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            string responseString;
            try
            {
                HttpResponseMessage response;
                response = await _retryPolicy.ExecuteAsync(async () =>
                         await CreateAndSendGetMessageAsync(url)
                    );
                if (response.IsSuccessStatusCode)
                {
                    responseString = await response.Content.ReadAsStringAsync();

                }
                else
                {
                    responseString = "NotFound";
                }
                return responseString;
            }
            catch (Exception ex)
            {
                _logger.LogError("Error contacting Graph", ex);
                return ex.Message;
            }
        }

        private async Task<HttpResponseMessage> CreateAndSendGetMessageAsync(string url)
        {
            HttpResponseMessage response;
            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            response = await _httpClient.SendAsync(requestMessage);
            return response;
        }
    }
}
