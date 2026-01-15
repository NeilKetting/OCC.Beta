using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;

namespace OCC.Client.Services.Repositories.ApiServices
{
    public class ApiHealthSafetyService : IHealthSafetyService
    {
        private readonly HttpClient _httpClient; 

        public ApiHealthSafetyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // --- Incidents ---
        public async Task<IEnumerable<Incident>> GetIncidentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Incident>>("api/Incidents") ?? new List<Incident>();
        }

        public async Task<Incident?> GetIncidentAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<Incident>($"api/Incidents/{id}");
        }

        public async Task<Incident?> CreateIncidentAsync(Incident incident)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Incidents", incident);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<Incident>();
            }
            return null;
        }

        public async Task<bool> UpdateIncidentAsync(Incident incident)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Incidents/{incident.Id}", incident);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteIncidentAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Incidents/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Audits ---
        public async Task<IEnumerable<HseqAudit>> GetAuditsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HseqAudit>>("api/HseqAudits") ?? new List<HseqAudit>();
        }

        public async Task<HseqAudit?> GetAuditAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<HseqAudit>($"api/HseqAudits/{id}");
        }

        public async Task<HseqAudit?> CreateAuditAsync(HseqAudit audit)
        {
            var response = await _httpClient.PostAsJsonAsync("api/HseqAudits", audit);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<HseqAudit>();
            }
            return null;
        }

        public async Task<bool> UpdateAuditAsync(HseqAudit audit)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/HseqAudits/{audit.Id}", audit);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<HseqAuditNonComplianceItem>> GetAuditDeviationsAsync(Guid auditId)
        {
             return await _httpClient.GetFromJsonAsync<IEnumerable<HseqAuditNonComplianceItem>>($"api/HseqAudits/{auditId}/deviations") ?? new List<HseqAuditNonComplianceItem>();
        }

        // --- Training ---
        public async Task<IEnumerable<HseqTrainingRecord>> GetTrainingRecordsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HseqTrainingRecord>>("api/HseqTraining") ?? new List<HseqTrainingRecord>();
        }

        public async Task<IEnumerable<HseqTrainingRecord>> GetExpiringTrainingAsync(int days)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HseqTrainingRecord>>($"api/HseqTraining/expiring/{days}") ?? new List<HseqTrainingRecord>();
        }

        public async Task<HseqTrainingRecord?> CreateTrainingRecordAsync(HseqTrainingRecord record)
        {
            var response = await _httpClient.PostAsJsonAsync("api/HseqTraining", record);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<HseqTrainingRecord>();
            }
            return null;
        }

        public async Task<bool> DeleteTrainingRecordAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/HseqTraining/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Documents ---
        public async Task<IEnumerable<HseqDocument>> GetDocumentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<HseqDocument>>("api/HseqDocuments") ?? new List<HseqDocument>();
        }

        public async Task<HseqDocument?> UploadDocumentAsync(HseqDocument document)
        {
             var response = await _httpClient.PostAsJsonAsync("api/HseqDocuments", document);
             if (response.IsSuccessStatusCode)
             {
                 return await response.Content.ReadFromJsonAsync<HseqDocument>();
             }
             return null;
        }

        public async Task<bool> DeleteDocumentAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/HseqDocuments/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Stats ---
        public async Task<HseqDashboardStats?> GetDashboardStatsAsync()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<HseqDashboardStats>("api/HseqStats/dashboard");
            }
            catch
            {
                return new HseqDashboardStats(); // Return empty on error
            }
        }
    }
}
