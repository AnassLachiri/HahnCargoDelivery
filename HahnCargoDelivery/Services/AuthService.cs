using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HahnCargoDelivery.Services;

public interface IAuthService
{
    public Task<LoginResponse> Login(LoginRequest loginRequest);
    public string GetToken();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly HahnCargoSimApiConfig _hahnCargoSimApiConfig;

    private string _jwtToken;

    public AuthService(HttpClient httpClient, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig)
    {
        if (hahnCargoSimApiConfig.Value == null)
        {
            throw new Exception("Configs can't be null");
        }

        _httpClient = httpClient;
        _hahnCargoSimApiConfig = hahnCargoSimApiConfig.Value;
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(_hahnCargoSimApiConfig.Uri + "user/login", content);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();
        var loginResp = JsonConvert.DeserializeObject<LoginResponse>(responseContent);
        
        if (loginResp == null)
        {
            throw new Exception("Login response can't be null!");
        }

        _jwtToken = loginResp.Token;

        return loginResp;
    }
    
    public string GetToken() => _jwtToken;
}