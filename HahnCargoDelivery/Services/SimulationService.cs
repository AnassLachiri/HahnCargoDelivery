using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using HahnCargoDelivery.Helpers;
using HahnCargoDelivery.Models;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Services;

public class SimulationService(IAuthService authService, IGridService gridService, IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig, ILogger<SimulationService> logger, IRabbitMqService rabbitMqService) : IHostedService
{
    private readonly SimulationState _simulationState = new SimulationState();

    private async Task InitializeSimulationState()
    {
        // Login to get auth token
        await authService.Login(new LoginRequest("Anass", "Hahn"));
        // Get the grid
        _simulationState.Grid = await gridService.GetGrid();
        _simulationState.IsSimulationStarted = true;

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeSimulationState();
        Console.WriteLine("-------------------------------------");
        var path = DjikstraHelper.GetShortestPath(_simulationState.Grid, 1, 35);
        var cost = DjikstraHelper.GetTotalCost(_simulationState.Grid, path);
        var time = DjikstraHelper.GetTotalTime(_simulationState.Grid, path);
        Console.WriteLine($"Path count : {path.Count}");
        Console.WriteLine($"Cost : {cost}");
        Console.WriteLine($"Time : {time}");
        Console.WriteLine("-------------------------------------");

        // await externalApiService.PostAsync(hahnCargoSimApiConfig.Value.Uri + "sim/start", null);

        await rabbitMqService.StartRabbitMqListener(order =>
        {
            // Do something with your order
            Console.WriteLine($"New Order with Id : {order.Id} and value : {order.Value}");
        }, cancellationToken);
        logger.LogInformation("RabbitMQListener started.");
        
        Console.WriteLine($"------------------ Test -----------------------");
        // while (_simulationState.IsSimulationStarted)
        // {
        //     foreach (var transporter in _simulationState.Transporters)
        //     {
        //         
        //         
        //     }
        // }
        
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        rabbitMqService.DestroyRabbitMqListener();
        return Task.CompletedTask;
    }
}