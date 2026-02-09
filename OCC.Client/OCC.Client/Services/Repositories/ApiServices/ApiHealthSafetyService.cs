using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;
using OCC.Shared.DTOs;
using System.Text.Json;

namespace OCC.Client.Services.Repositories.ApiServices
{
    public class ApiHealthSafetyService : IHealthSafetyService
    {
        private readonly HttpClient _httpClient; 
        private readonly JsonSerializerOptions _options;

        public ApiHealthSafetyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
        }

        // --- Incidents ---
        public async Task<IEnumerable<IncidentSummaryDto>> GetIncidentsAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<IncidentSummaryDto>>("api/Incidents", _options) ?? new List<IncidentSummaryDto>();
        }

        public async Task<IncidentDto?> GetIncidentAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<IncidentDto>($"api/Incidents/{id}", _options);
        }

        public async Task<IncidentDto?> CreateIncidentAsync(Incident incident)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Incidents", incident, _options);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IncidentDto>(_options);
            }
            return null;
        }

        public async Task<bool> UpdateIncidentAsync(Incident incident)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Incidents/{incident.Id}", incident, _options);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteIncidentAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Incidents/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IncidentPhotoDto?> UploadIncidentPhotoAsync(IncidentPhoto metadata, System.IO.Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(metadata.IncidentId.ToString()), nameof(IncidentPhoto.IncidentId));
            content.Add(new StringContent(metadata.Description ?? ""), nameof(IncidentPhoto.Description));

            if (fileStream.CanSeek) fileStream.Position = 0;
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/Incidents/photos", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IncidentPhotoDto>(_options);
            }
            return null;
        }

        public async Task<bool> DeleteIncidentPhotoAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Incidents/photos/{id}");
            return response.IsSuccessStatusCode;
        }

        // --- Audits ---
        public async Task<IEnumerable<AuditSummaryDto>> GetAuditsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/HseqAudits");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<IEnumerable<AuditSummaryDto>>(_options) ?? new List<AuditSummaryDto>();
                }
                System.Diagnostics.Debug.WriteLine($"[ApiHealthSafetyService] GetAudits Failed: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                 System.Diagnostics.Debug.WriteLine($"[ApiHealthSafetyService] GetAudits Exception: {ex.Message}");
            }
            return new List<AuditSummaryDto>();
        }

        public async Task<AuditDto?> GetAuditAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<AuditDto>($"api/HseqAudits/{id}", _options);
        }

        public async Task<AuditDto?> CreateAuditAsync(AuditDto audit)
        {
            var response = await _httpClient.PostAsJsonAsync("api/HseqAudits", audit);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuditDto>();
            }
            return null;
        }

        public async Task<bool> UpdateAuditAsync(AuditDto audit)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/HseqAudits/{audit.Id}", audit);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteAuditAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/HseqAudits/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<AuditNonComplianceItemDto>> GetAuditDeviationsAsync(Guid auditId)
        {
             return await _httpClient.GetFromJsonAsync<IEnumerable<AuditNonComplianceItemDto>>($"api/HseqAudits/{auditId}/deviations") ?? new List<AuditNonComplianceItemDto>();
        }

        public async Task<AuditAttachmentDto?> UploadAuditAttachmentAsync(HseqAuditAttachment metadata, System.IO.Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(metadata.AuditId.ToString()), nameof(HseqAuditAttachment.AuditId));
            if (metadata.NonComplianceItemId.HasValue)
                content.Add(new StringContent(metadata.NonComplianceItemId.Value.ToString()), nameof(HseqAuditAttachment.NonComplianceItemId));
            content.Add(new StringContent(metadata.FileName ?? ""), nameof(HseqAuditAttachment.FileName));
            content.Add(new StringContent(metadata.UploadedBy ?? ""), nameof(HseqAuditAttachment.UploadedBy));

            if (fileStream.CanSeek) fileStream.Position = 0;
            using var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync("api/HseqAudits/attachments", content);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<AuditAttachmentDto>(_options);
            }
            return null;
        }

        public async Task<bool> DeleteAuditAttachmentAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/HseqAudits/attachments/{id}");
            return response.IsSuccessStatusCode;
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

        public async Task<bool> UpdateTrainingRecordAsync(HseqTrainingRecord record)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/HseqTraining/{record.Id}", record);
            return response.IsSuccessStatusCode;
        }

        public async Task<string?> UploadCertificateAsync(System.IO.Stream fileStream, string fileName)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                
                // Reset stream position if possible
                if (fileStream.CanSeek) fileStream.Position = 0;
                
                using var streamContent = new StreamContent(fileStream);
                content.Add(streamContent, "file", fileName);

                var response = await _httpClient.PostAsync("api/HseqTraining/upload", content);
                if (response.IsSuccessStatusCode)
                {
                    // Expecting JSON: { "url": "/uploads/..." }
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (result.TryGetProperty("url", out var urlProp))
                    {
                        return urlProp.GetString();
                    }
                    else if (result.TryGetProperty("Url", out var urlPropCase))
                    {
                        return urlPropCase.GetString();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Upload Exception: {ex.Message}");
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

        public async Task<HseqDocument?> UploadDocumentAsync(HseqDocument metadata, System.IO.Stream fileStream, string fileName)
        {
             using var content = new MultipartFormDataContent();
             content.Add(new StringContent(metadata.Title ?? ""), nameof(HseqDocument.Title));
             content.Add(new StringContent(metadata.Category.ToString()), nameof(HseqDocument.Category));
             content.Add(new StringContent(metadata.UploadedBy ?? ""), nameof(HseqDocument.UploadedBy));
             content.Add(new StringContent(metadata.Version ?? "1.0"), nameof(HseqDocument.Version));
             
             // Reset stream position if possible
             if (fileStream.CanSeek) fileStream.Position = 0;
             using var streamContent = new StreamContent(fileStream);
             content.Add(streamContent, "file", fileName);

             var response = await _httpClient.PostAsync("api/HseqDocuments", content);
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
                var response = await _httpClient.GetAsync("api/HseqStats/dashboard");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<HseqDashboardStats>(_options);
                }
                System.Diagnostics.Debug.WriteLine($"[ApiHealthSafetyService] GetDashboardStats Failed: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ApiHealthSafetyService] GetDashboardStats Error: {ex.Message}");
            }
            return new HseqDashboardStats();
        }

        public async Task<IEnumerable<HseqSafeHourRecord>> GetPerformanceHistoryAsync(int? year = null)
        {
             return await _httpClient.GetFromJsonAsync<IEnumerable<HseqSafeHourRecord>>($"api/HseqStats/history/{year}") ?? new List<HseqSafeHourRecord>();
        }
    }
}
