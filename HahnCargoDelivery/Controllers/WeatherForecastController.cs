using HahnCargoDelivery.Dtos.Authentication;
using HahnCargoDelivery.Services;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ILoginService _loginService;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, ILoginService loginService)
    {
        _logger = logger;
        _loginService = loginService;
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
        return Ok(await _loginService.Login(new LoginRequest("Anass", "Hahn")));
    }
}