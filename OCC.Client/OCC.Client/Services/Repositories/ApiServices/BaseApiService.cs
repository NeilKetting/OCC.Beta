using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OCC.Client.Services.Repositories.Interfaces;
using OCC.Client.Services.Interfaces;
using OCC.Client.Services.Infrastructure;
using System.Linq;

namespace OCC.Client.Services.Repositories.ApiServices
{
    public abstract class BaseApiService<T> : IRepository<T> where T : class, Shared.Models.IEntity
    {
        protected readonly HttpClient _httpClient;
        protected readonly IAuthService _authService;
        
        /// <summary>
        /// The resource name, e.g. "Projects" or "Users"
        /// </summary>
        protected abstract string ApiEndpoint { get; }

        public BaseApiService(IAuthService authService)
        {
            _authService = authService;
            _httpClient = new HttpClient();
        }

        protected string GetFullUrl(string path)
        {
            var baseUrl = ConnectionSettings.Instance.ApiBaseUrl;
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            return $"{baseUrl}{path}";
        }

        protected virtual void EnsureAuthorization()
        {
            var token = _authService.AuthToken;
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            EnsureAuthorization();
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<T>>(GetFullUrl($"api/{ApiEndpoint}"));
            return result ?? Enumerable.Empty<T>();
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            EnsureAuthorization();
            try
            {
                return await _httpClient.GetFromJsonAsync<T>(GetFullUrl($"api/{ApiEndpoint}/{id}"));
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public virtual async Task AddAsync(T entity)
        {
            EnsureAuthorization();
            var response = await _httpClient.PostAsJsonAsync(GetFullUrl($"api/{ApiEndpoint}"), entity);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Details: {errorContent}", null, response.StatusCode);
            }
        }

        public virtual async Task UpdateAsync(T entity)
        {
            EnsureAuthorization();
            var response = await _httpClient.PutAsJsonAsync(GetFullUrl($"api/{ApiEndpoint}/{entity.Id}"), entity);
            
            if (!response.IsSuccessStatusCode)
            {
                 if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                 {
                     throw new OCC.Client.Infrastructure.Exceptions.ConcurrencyException();
                 }

                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Details: {errorContent}", null, response.StatusCode);
            }
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            EnsureAuthorization();
            var response = await _httpClient.DeleteAsync(GetFullUrl($"api/{ApiEndpoint}/{id}"));
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.ReasonPhrase}). Details: {errorContent}", null, response.StatusCode);
            }
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            // Note: For true efficiency, this should push the query to the API (e.g. OData).
            // For now, valid implementation is to fetch all and filter client-side.
            var allItems = await GetAllAsync();
            return allItems.Where(predicate.Compile());
        }
    }
}
