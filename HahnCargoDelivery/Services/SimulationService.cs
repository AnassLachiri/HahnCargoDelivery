using System.Text;
using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using HahnCargoDelivery.Helpers;
using HahnCargoDelivery.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HahnCargoDelivery.Services;

public class SimulationService(IAuthService authService, IGridService gridService, IRabbitMQService rabbitMqService, IExternalApiService externalApiService, IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig) : IHostedService
{
    private readonly SimulationState _simulationState = new SimulationState();
    
    private async Task InitializeSimulationState()
    {
        // Login to get auth token
        await authService.Login(new LoginRequest("Anass", "Hahn"));
        // Get the grid
        _simulationState.grid = await gridService.GetGrid();

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await InitializeSimulationState();
        Console.WriteLine("-------------------------------------");
        var path = DjikstraHelper.GetShortestPath(_simulationState.grid, 1, 35);
        var cost = DjikstraHelper.GetTotalCost(_simulationState.grid, path);
        var time = DjikstraHelper.GetTotalTime(_simulationState.grid, path);
        Console.WriteLine($"Path count : {path.Count}");
        Console.WriteLine($"Cost : {cost}");
        Console.WriteLine($"Time : {time}");
        Console.WriteLine("-------------------------------------");

        await externalApiService.PostAsync(hahnCargoSimApiConfig.Value.Uri + "sim/start", null);
        
        var consumer = new EventingBasicConsumer(rabbitMqService.GetChannel());
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            // Deserialize and process the message
            var order = JsonConvert.DeserializeObject<Order>(message);
            if (order == null)
            {
                throw new Exception("Order can't be null");
            }

            // Do something with your order
            Console.WriteLine($"New Order with Id : {order.Id} and value : {order.Value}");
        };

        rabbitMqService.GetChannel().BasicConsume(queue: "HahnCargoSim_NewOrders",
            autoAck: true,
            consumer: consumer);
        
        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        rabbitMqService.GetChannel().Close();
        rabbitMqService.GetConnection().Close();
        return Task.CompletedTask;
    }
}