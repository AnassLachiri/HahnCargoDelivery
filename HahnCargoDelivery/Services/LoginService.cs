using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace HahnCargoDelivery.Services;

public interface ILoginService
{
    public Task<LoginResponse> Login(LoginRequest loginRequest);
}

public class LoginService : ILoginService
{
    private readonly IExternalApiService _externalApiService;
    private readonly HahnCargoSimApiConfig _hahnCargoSimApiConfig;

    public LoginService(IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig)
    {
        if (hahnCargoSimApiConfig.Value == null)
        {
            throw new Exception("Configs can't be null");
        }

        _externalApiService = externalApiService;
        _hahnCargoSimApiConfig = hahnCargoSimApiConfig.Value;
    }

    public async Task<LoginResponse> Login(LoginRequest loginRequest)
    {
        var content = new StringContent(JsonConvert.SerializeObject(loginRequest), System.Text.Encoding.UTF8, "application/json");
        var response = await _externalApiService.PostAsync<LoginResponse>(_hahnCargoSimApiConfig.Uri + "user/login", content);
        if (response == null)
        {
            throw new Exception("Login response can't be null!");
        }

        return response;
    }
}