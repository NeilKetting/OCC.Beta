using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OCC.Client.Services
{
    public class BugReportService : IBugReportService
    {
        private readonly HttpClient _httpClient;
        private readonly IAuthService _authService;
        private readonly IPermissionService _permissionService;
        
        public BugReportService(HttpClient httpClient, IAuthService authService, IPermissionService permissionService)
        {
            _httpClient = httpClient;
            _authService = authService;
            _permissionService = permissionService;
        }

        private void EnsureAuthorization()
        {
            var token = _authService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task SubmitBugAsync(BugReport report)
        {
            EnsureAuthorization();
            var response = await _httpClient.PostAsJsonAsync("api/BugReports", report);
            response.EnsureSuccessStatusCode();
        }

        public async Task<List<BugReport>> GetBugReportsAsync()
        {
            EnsureAuthorization();
            return await _httpClient.GetFromJsonAsync<List<BugReport>>("api/BugReports") ?? new List<BugReport>();
        }

        public async Task AddCommentAsync(Guid bugId, string comment, string? status)
        {
            EnsureAuthorization();
            var currentUser = _authService.CurrentUser;
            var bugComment = new BugComment
            {
                BugReportId = bugId,
                Content = comment,
                AuthorName = (currentUser?.FirstName + " " + currentUser?.LastName)?.Trim() ?? "System",
                AuthorEmail = currentUser?.Email ?? "",
                IsDevComment = _permissionService.IsDev,
                CreatedAtUtc = DateTime.UtcNow
            };

            var url = $"api/BugReports/{bugId}/comments";
            if (!string.IsNullOrEmpty(status))
            {
                url += $"?status={status}";
            }

            var response = await _httpClient.PostAsJsonAsync(url, bugComment);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteBugAsync(Guid bugId)
        {
            EnsureAuthorization();
            var response = await _httpClient.DeleteAsync($"api/BugReports/{bugId}");
            response.EnsureSuccessStatusCode();
        }
    }
}
