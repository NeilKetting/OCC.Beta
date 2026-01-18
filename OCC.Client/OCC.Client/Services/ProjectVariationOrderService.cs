using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using OCC.Client.Services.Interfaces;
using OCC.Shared.Models;

namespace OCC.Client.Services
{
    public class ProjectVariationOrderService : IProjectVariationOrderService
    {
        private readonly HttpClient _httpClient;

        public ProjectVariationOrderService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<ProjectVariationOrder>> GetVariationOrdersAsync(Guid? projectId = null)
        {
            var url = "api/ProjectVariationOrders";
            if (projectId.HasValue)
            {
                url += $"?projectId={projectId.Value}";
            }
            return await _httpClient.GetFromJsonAsync<IEnumerable<ProjectVariationOrder>>(url) ?? new List<ProjectVariationOrder>();
        }

        public async Task<ProjectVariationOrder> GetVariationOrderAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<ProjectVariationOrder>($"api/ProjectVariationOrders/{id}") ?? throw new Exception("Variation order not found");
        }

        public async Task<ProjectVariationOrder> CreateVariationOrderAsync(ProjectVariationOrder variationOrder)
        {
            var response = await _httpClient.PostAsJsonAsync("api/ProjectVariationOrders", variationOrder);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ProjectVariationOrder>() ?? throw new Exception("Failed to deserialize created variation order");
        }

        public async Task UpdateVariationOrderAsync(ProjectVariationOrder variationOrder)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/ProjectVariationOrders/{variationOrder.Id}", variationOrder);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteVariationOrderAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/ProjectVariationOrders/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
