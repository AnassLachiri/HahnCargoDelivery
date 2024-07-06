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

public class ExternalApiService(HttpClient httpClient) : IExternalApiService
{
    public async Task<T?> GetAsync<T>(string requestUri)
    {
        var response = await httpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }

    public async Task<T?> PostAsync<T>(string requestUri, HttpContent content)
    {
        var response = await httpClient.PostAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
    
    public async Task<T?> PutAsync<T>(string requestUri, HttpContent content)
    {
        var response = await httpClient.PutAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
    
    public async Task<T?> PatchAsync<T>(string requestUri, HttpContent content)
    {
        var response = await httpClient.PutAsync(requestUri, content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
    
    public async Task<T?> DeleteAsync<T>(string requestUri)
    {
        var response = await httpClient.DeleteAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(responseContent);
    }
}