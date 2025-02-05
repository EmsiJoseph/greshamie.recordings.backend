using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using backend.Constants;
using backend.Models;

namespace backend.Services.ClarifyGoAutoLogin
{
    public class ClarifyGoAutoLoginService : IClarifyGoAutoLoginService
    {
        private readonly HttpClient _httpClient;
        private bool _isLoggedIn = false;

        public ClarifyGoAutoLoginService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var loginData = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string, string>("Username", username),
            new KeyValuePair<string, string>("Password", password),
            new KeyValuePair<string, string>("RememberMe", "true")  // If available
        });

                var request = new HttpRequestMessage(HttpMethod.Post, ClarifyGoApiEndpoints.Audits.LoginUrl)
                {
                    Content = loginData
                };
                request.Headers.Add("User-Agent", "Mozilla/5.0"); // Mimic a browser request if needed

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    _isLoggedIn = true;
                    Console.WriteLine("Login successful, session established.");
                    return true;
                }

                // Log response for debugging
                string errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Login failed: {response.StatusCode} - {errorResponse}");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"HTTP Request error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }

            return false;
        }

        public async Task<List<AuditLog>> FetchAuditLogsAsync(string fromDate, string toDate, string eventType)
        {
            if (!_isLoggedIn)
            {
                Console.WriteLine("Not logged in. Please login first.");
                return null;
            }

            var url = ClarifyGoApiEndpoints.Audits.Search(fromDate, toDate, eventType);

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var auditData = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrWhiteSpace(auditData))
                {
                    try
                    {
                        var auditLogs = JsonSerializer.Deserialize<List<AuditLog>>(auditData, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return auditLogs;
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"JSON deserialization error: {ex.Message}");
                        return null;
                    }
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch audit logs: {response.StatusCode}");
            }

            return null;
        }
    }
}
