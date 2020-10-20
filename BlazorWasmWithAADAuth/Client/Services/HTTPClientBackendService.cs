using BlazorWasmWithAADAuth.Shared;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BlazorWasmWithAADAuth.Client.Services
{
    public class HTTPClientBackendService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
        public HTTPClientBackendService(HttpClient httpClient)
        {
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

        public async Task<WeatherForecast[]> CallGetApiAsync(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url is empty or null", nameof(url));
            }
            WeatherForecast[] weatherForecasts = new WeatherForecast[0];
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            try
            {
                HttpResponseMessage response;
                response = await _retryPolicy.ExecuteAsync(async () =>
                         await CreateAndSendGetMessageAsync(url)
                    );
                weatherForecasts = JsonConvert.DeserializeObject<WeatherForecast[]>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                //TODO handle error
            }
            return weatherForecasts;
        }

        public async Task<WeatherForecast[]> CallPostAPIAsync(string url, string csrfCookieValue, string jsonPayload = null)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentNullException("url is empty or null", nameof(url));
            }
            WeatherForecast[] weatherForecasts = new WeatherForecast[0];
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            try
            {
                HttpResponseMessage response;
                HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                if (!string.IsNullOrWhiteSpace(csrfCookieValue))
                {
                    requestMessage.Headers.Add("X-CSRF-TOKEN", csrfCookieValue);
                }
                if(!string.IsNullOrWhiteSpace(jsonPayload))
                {
                    requestMessage.Content = new StringContent(jsonPayload,
                    Encoding.UTF8, "application/json"); 
                }
                response = await _retryPolicy.ExecuteAsync(async () =>
                         await SendMessageAsync(requestMessage)
                    );
                weatherForecasts = JsonConvert.DeserializeObject<WeatherForecast[]>(await response.Content.ReadAsStringAsync());
            }
            catch (Exception ex)
            {
                //TODO handle error
            }
            return weatherForecasts;
        }

        private async Task<HttpResponseMessage> SendMessageAsync(HttpRequestMessage requestMessage)
        {
            HttpResponseMessage response;
            response = await _httpClient.SendAsync(requestMessage);
            return response;
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
