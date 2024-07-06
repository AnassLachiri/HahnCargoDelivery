using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using HahnCargoDelivery.Models;
using HahnCargoDelivery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IAuthService _authService;
    private readonly IExternalApiService _externalApiService;
    private readonly HahnCargoSimApiConfig _hahnCargoSimApiConfig;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IAuthService authService, IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig)
    {
        if (hahnCargoSimApiConfig.Value == null)
        {
            throw new Exception("Configs can't be null");
        }

        _hahnCargoSimApiConfig = hahnCargoSimApiConfig.Value;
        _logger = logger;
        _authService = authService;
        _externalApiService = externalApiService;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<WeatherForecast> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
    }
    
    [HttpPost(Name = "Login")]
    public async Task<ActionResult<LoginResponse>> Login()
    {
        return Ok(await _authService.Login(new LoginRequest("Anass", "Hahn")));
    }
    
    [HttpGet("Grid")]
    public async Task<ActionResult<Grid>> GetGrid()
    {
        return Ok(await _externalApiService.GetAsync<Grid>(_hahnCargoSimApiConfig.Uri + "Grid/Get"));
    }
}