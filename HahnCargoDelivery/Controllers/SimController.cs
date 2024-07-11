using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using HahnCargoDelivery.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class SimController(SimulationService simulationService, IAuthService authService, IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig)
{
    
    [HttpPost]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest loginRequest)
    {
        return await authService.Login(loginRequest);
    }
    
    [HttpPost]
    public async Task StartSimulation()
    {
        if (!simulationService.SimInitialized)
        {
            await simulationService.InitializeSimulationState();
        }
        await simulationService.StartSimulation();
    }
    
    [HttpPost]
    public async Task  StopSimulation()
    {
        await simulationService.StopSimulation();
        
    }
    
    [HttpGet]
    public async Task<ActionResult<SimulationStateDto>> GetSimulationState()
    {
        var state = simulationService.GetSimulationState();
        var amountCoins = await externalApiService.GetAsync<int>(hahnCargoSimApiConfig.Value.Uri + "User/CoinAmount");
        var stateDto = new SimulationStateDto()
        {
            IsSimulationStarted = state.IsSimulationStarted,
            Grid = state.Grid,
            Transporters = state.Transporters,
            CoinAmount = amountCoins,
            DeliveredOrders = state.DeliveredOrders
        };

        return stateDto;
    }
}