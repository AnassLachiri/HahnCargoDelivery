using HahnCargoDelivery.Configs;
using HahnCargoDelivery.Dtos.Authentication;
using HahnCargoDelivery.Helpers;
using HahnCargoDelivery.Models;
using Microsoft.Extensions.Options;

namespace HahnCargoDelivery.Services;

public class SimulationService(
        IAuthService authService, 
        IGridService gridService, 
        IExternalApiService externalApiService, 
        IOptions<HahnCargoSimApiConfig> hahnCargoSimApiConfig, 
        ILogger<SimulationService> logger, 
        IRabbitMqService rabbitMqService,
        ITransporterService transporterService,
        IOrderService orderService) : BackgroundService
{
    private static readonly SimulationState _simulationState = new SimulationState();

    private async Task InitializeSimulationState()
    {
        // Login to get auth token
        await authService.Login(new LoginRequest("Anass", "Hahn"));
        // Get the grid
        _simulationState.Grid = await gridService.GetGrid();
        _simulationState.Transporters = [];
        _simulationState.IsSimulationStarted = true;
        await externalApiService.PostAsync(hahnCargoSimApiConfig.Value.Uri + "sim/start", null);

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

        await rabbitMqService.StartRabbitMqListener(async order =>
        {
            Console.WriteLine($"Enqueuing New Order with Id : {order.Id} and value : {order.Value}");
            _simulationState.Orders.Enqueue(order);
        }, stoppingToken);
        logger.LogInformation("RabbitMQListener started.");
        
        Console.WriteLine($"------------------ Test -----------------------");
        while (_simulationState.IsSimulationStarted)
        {
            await HandleOrders(stoppingToken);
            await MoveTransporters(stoppingToken);
            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task HandleOrders(CancellationToken cancellationToken)
    {
        var batchCount = 10;
        while (batchCount > 0 && _simulationState.Orders.Count != 0  && !cancellationToken.IsCancellationRequested)
        {
            if (_simulationState.Orders.TryDequeue(out var order))
            {
                if (_simulationState.Transporters.Count == 0)
                {
                    var path = DjikstraHelper.GetShortestPath(_simulationState.Grid, order.OriginNodeId,
                        order.TargetNodeId);
                    var cost = DjikstraHelper.GetTotalCost(_simulationState.Grid, path);
        
                    if (cost <= 1000)
                    {
                        var availableOrders = await orderService.GetAllOrders();
                        var o = availableOrders.FirstOrDefault(o => o.Id == order.Id);
                        if (o == null) continue;
                        await orderService.AcceptOrder(order.Id);
                        var transporterId = await transporterService.Buy(order.OriginNodeId);
                        Console.WriteLine($"Transporter bought with transporterId : {transporterId}.");
                        Console.WriteLine($"Accepting order number: {order.Id}.");
                        Console.WriteLine($"Order number: {order.Id} accepted.");
            
                        var pathQueue = new Queue<int>(path);
                        pathQueue.Dequeue();
                        _simulationState.Transporters.Add(new TransporterInfo { Id = transporterId, Orders = [order], PathRemained = pathQueue});
                    }
                }
            }
            batchCount--;
        }
    }
    
    private async Task MoveTransporters(CancellationToken cancellationToken)
    {
        foreach (var transporter in _simulationState.Transporters)
        {
            logger.LogInformation($"Handling transporter {transporter.Id}.");
            var cargoTransporter = await transporterService.Get(transporter.Id);
            if (cargoTransporter.InTransit)
            {
                logger.LogInformation($"Transporter {transporter.Id} is in transit.");
                continue;
            }
            
            if (transporter.PathRemained.Count > 0)
            {
                var targetNode = transporter.PathRemained.Dequeue();
                await transporterService.Move(transporter.Id, targetNode);
                logger.LogInformation($"Transporter {transporter.Id} moved to node {targetNode}.");
                var finishedOrder = transporter.Orders.Find(o => o.TargetNodeId==targetNode);
                if (finishedOrder != null)
                {
                    transporter.Orders.Remove(finishedOrder);
                    logger.LogInformation($"Order {finishedOrder.Id} delivered and removed from orders.");
                }
            }
            else
            {
                logger.LogInformation($"Finished delivering all orders for transporter number {transporter.Id}.");
                transporter.Orders.Clear();
            }
            
        }
    }

    public SimulationState GetSimulationState()
    {
        return _simulationState;
    }

}