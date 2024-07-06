using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace HahnCargoDelivery.Services;


public interface IExternalApiService
{
    public Task<T?> GetAsync<T>(string requestUri);
    public Task<T?> PostAsync<T>(string requestUri, HttpContent content);
    public Task<T?> PutAsync<T>(string requestUri, HttpContent content);
    public Task<T?> PatchAsync<T>(string requestUri, HttpContent content);
    public Task<T?> DeleteAsync<T>(string requestUri);
}

public class ExternalApiService(HttpClient httpClient, IAuthService authService) : IExternalApiService
{
    public async Task<T?> GetAsync<T>(string requestUri)
    {
        var token = authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }

    public async Task<T?> PostAsync<T>(string requestUri, HttpContent content)
    {
        var token = authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
    
    public async Task<T?> PutAsync<T>(string requestUri, HttpContent content)
    {
        var token = authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.PutAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
    
    public async Task<T?> PatchAsync<T>(string requestUri, HttpContent content)
    {
        var token = authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.PutAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
    
    public async Task<T?> DeleteAsync<T>(string requestUri)
    {
        var token = authService.GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        var response = await httpClient.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
}