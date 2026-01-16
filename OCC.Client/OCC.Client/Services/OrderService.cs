using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Managers.Interfaces;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Shared.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Linq;

namespace OCC.Client.Services
{
    public class OrderService : IOrderService
    {
        private readonly HttpClient _httpClient;
        private readonly IInventoryService _inventoryService;
        private readonly IAuthService _authService;

        public OrderService(HttpClient httpClient, IInventoryService inventoryService, IAuthService authService)
        {
            _httpClient = httpClient;
            _inventoryService = inventoryService;
            _authService = authService;
        }

        private void EnsureAuthorization()
        {
            var token = _authService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<List<Order>> GetOrdersAsync()
        {
            EnsureAuthorization();
            return await _httpClient.GetFromJsonAsync<List<Order>>("api/Orders") ?? new List<Order>();
        }

        public async Task<Order?> GetOrderAsync(Guid id)
        {
             EnsureAuthorization();
             return await _httpClient.GetFromJsonAsync<Order>($"api/Orders/{id}");
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            EnsureAuthorization();
            var response = await _httpClient.PostAsJsonAsync("api/Orders", order);
            
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Failed to create order: {response.StatusCode} - {error}");
            }
            
            return await response.Content.ReadFromJsonAsync<Order>() ?? order;
        }

        public async Task UpdateOrderAsync(Order order)
        {
             EnsureAuthorization();
             var response = await _httpClient.PutAsJsonAsync($"api/Orders/{order.Id}", order);
             
             if (!response.IsSuccessStatusCode)
             {
                 var error = await response.Content.ReadAsStringAsync();
                 throw new HttpRequestException($"Failed to update order: {response.StatusCode} - {error}");
             }
        }

        public async Task ReceiveOrderAsync(Order order, List<OrderLine> updatedLines)
        {
             EnsureAuthorization();
             var response = await _httpClient.PostAsJsonAsync($"api/Orders/{order.Id}/receive", updatedLines);
             response.EnsureSuccessStatusCode();

             // Update local order object with response if needed, but the caller usually reloads
             var updatedOrder = await response.Content.ReadFromJsonAsync<Order>();
             if (updatedOrder != null) 
             {
                 order.Status = updatedOrder.Status;
                 // Sync lines? Usually List ViewModel reloads, but good to keep local object fresh
                 foreach(var line in updatedOrder.Lines)
                 {
                     var local = order.Lines.FirstOrDefault(l => l.Id == line.Id);
                     if (local != null) local.QuantityReceived = line.QuantityReceived;
                 }
             }
        }

        public async Task DeleteOrderAsync(Guid id)
        {
             var response = await _httpClient.DeleteAsync($"api/Orders/{id}");
             response.EnsureSuccessStatusCode();
        }
    }
}
