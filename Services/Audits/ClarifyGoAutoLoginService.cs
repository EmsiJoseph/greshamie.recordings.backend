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
            var loginData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Username", username),
                new KeyValuePair<string, string>("Password", password),
                new KeyValuePair<string, string>("RememberMe", "true")  // If available
            });

            var response = await _httpClient.PostAsync(ClarifyGoApiEndpoints.Audits.LoginUrl, loginData);

            if (response.IsSuccessStatusCode)
            {
                _isLoggedIn = true;
                Console.WriteLine("Login successful, session established.");
                return true;
            }

            Console.WriteLine($"Login failed: {response.StatusCode}");
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
