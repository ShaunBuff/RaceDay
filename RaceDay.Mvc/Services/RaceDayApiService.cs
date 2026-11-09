using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;

namespace RaceDay.Mvc.Services
{
    public class RaceDayApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceDayApiService(
            HttpClient httpClient,
            IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private void AddJwtToken()
        {
            var token = _httpContextAccessor
                .HttpContext?
                .Session
                .GetString("Token");

            _httpClient.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer",
                        token);
            }
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            AddJwtToken();

            return await _httpClient.GetFromJsonAsync<T>(endpoint);
        }

        public async Task<HttpResponseMessage> PostAsync<T>(
            string endpoint,
            T data)
        {
            AddJwtToken();

            return await _httpClient.PostAsJsonAsync(endpoint, data);
        }

        public async Task<HttpResponseMessage> PutAsync<T>(
            string endpoint,
            T data)
        {
            AddJwtToken();

            return await _httpClient.PutAsJsonAsync(endpoint, data);
        }

        public async Task<HttpResponseMessage> DeleteAsync(
            string endpoint)
        {
            AddJwtToken();

            return await _httpClient.DeleteAsync(endpoint);
        }
    }
}