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
    public class ApiEmployeeService : IEmployeeService
    {
        private readonly HttpClient _httpClient; 
        private readonly JsonSerializerOptions _options;

        public ApiEmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };
        }

        public async Task<IEnumerable<EmployeeSummaryDto>> GetEmployeesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<EmployeeSummaryDto>>("api/Employees", _options) ?? new List<EmployeeSummaryDto>();
        }

        public async Task<EmployeeDto?> GetEmployeeAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<EmployeeDto>($"api/Employees/{id}", _options);
        }

        public async Task<EmployeeDto?> CreateEmployeeAsync(Employee employee)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Employees", employee, _options);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<EmployeeDto>(_options);
            }
            return null;
        }

        public async Task<bool> UpdateEmployeeAsync(Employee employee)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/Employees/{employee.Id}", employee, _options);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteEmployeeAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/Employees/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
