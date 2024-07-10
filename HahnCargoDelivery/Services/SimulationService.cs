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
                    var time = DjikstraHelper.GetTotalTime(_simulationState.Grid, path);
                    
                    
                    Console.WriteLine("----------------------------------------------------------");
                    Console.WriteLine($"order.ExpirationDate => {order.ExpirationDateUtc}");
                    Console.WriteLine($"DateTime.Now => {DateTime.Now}");
                    Console.WriteLine($"time => {time}");
                    Console.WriteLine($"DateTime.Now + time => {DateTime.Now + time}");
                    Console.WriteLine("----------------------------------------------------------");

                    if (cost <= 1000 && order.ExpirationDateUtc > DateTime.Now + time)
                    {
                        if ((await orderService.GetAllOrders()).Find(o => o.Id == order.Id) == null) continue;
                        await orderService.AcceptOrder(order.Id);
                        var transporterId = await transporterService.Buy(order.OriginNodeId);
                        Console.WriteLine($"Transporter bought with transporterId : {transporterId}.");
                        Console.WriteLine($"Accepting order number: {order.Id}.");
                        Console.WriteLine($"Order number: {order.Id} accepted.");

                        var pathQueue = new Queue<int>(path);
                        pathQueue.Dequeue();
                        _simulationState.Transporters.Add(new TransporterInfo
                            { Id = transporterId, Orders = [order], RemainingPath = pathQueue });
                    }
                }
                else
                {
                    foreach (var transporter in _simulationState.Transporters)
                    {
                        var ordersLoad = transporter.Orders.Sum(o => o.Load);
                        var cargoTransporter = await transporterService.Get(transporter.Id);
                        
                        var path = DjikstraHelper.GetShortestPath(_simulationState.Grid, transporter.RemainingPath.Peek(),
                            order.TargetNodeId);
                        var time = DjikstraHelper.GetTotalTime(_simulationState.Grid, path);
                        
                        if ((cargoTransporter.Capacity - ordersLoad) > order.Load && order.ExpirationDateUtc > DateTime.Now + time)
                        {
                            if (transporter.RemainingPath.Contains(order.OriginNodeId))
                            {
                                if (transporter.RemainingPath.Contains(order.TargetNodeId))
                                {
                                    if ((await orderService.GetAllOrders()).Find(o => o.Id == order.Id) == null) continue;
                                    await orderService.AcceptOrder(order.Id);
                                    transporter.Orders.Add(order);
                                    break;
                                }

                                var pathToAdd =
                                    DjikstraHelper.GetShortestPath(_simulationState.Grid, transporter.RemainingPath.ToArray().Last(), order.TargetNodeId);
                                var cost = DjikstraHelper.GetTotalCost(_simulationState.Grid, pathToAdd);
                                if (cost < order.Value)
                                {
                                    if ((await orderService.GetAllOrders()).Find(o => o.Id == order.Id) == null) continue;
                                    await orderService.AcceptOrder(order.Id);
                                    transporter.Orders.Add(order);
                                    pathToAdd.RemoveAt(0);
                                    foreach (var node in pathToAdd)
                                    {
                                        transporter.RemainingPath.Enqueue(node);
                                    }
                                    break;
                                }
                            }
                            
                            var firstPathToAdd =
                                DjikstraHelper.GetShortestPath(_simulationState.Grid, transporter.RemainingPath.ToArray().Last(), order.OriginNodeId);
                            var secondPathToAdd =
                                DjikstraHelper.GetShortestPath(_simulationState.Grid, order.OriginNodeId, order.TargetNodeId);
                            var firstPathCost = DjikstraHelper.GetTotalCost(_simulationState.Grid, firstPathToAdd);
                            var secondPathCost = DjikstraHelper.GetTotalCost(_simulationState.Grid, secondPathToAdd);

                            if ((firstPathCost + secondPathCost) < order.Value)
                            {
                                if ((await orderService.GetAllOrders()).Find(o => o.Id == order.Id) == null) continue;
                                await orderService.AcceptOrder(order.Id);
                            
                                firstPathToAdd.RemoveAt(0);
                                secondPathToAdd.RemoveAt(0);
                                transporter.Orders.Add(order);
                                foreach (var node in firstPathToAdd)
                                {
                                    transporter.RemainingPath.Enqueue(node);
                                }
                                foreach (var node in secondPathToAdd)
                                {
                                    transporter.RemainingPath.Enqueue(node);
                                }

                                break;   
                            }
                        }
                    }

                    if (!_simulationState.Transporters.Exists(t => t.Orders.Exists(o => o.Id == order.Id)))
                    {
                        var amountCoins = await externalApiService.GetAsync<int>(hahnCargoSimApiConfig.Value.Uri + "User/CoinAmount");
                        var remindPathCost = _simulationState.Transporters.Sum(t =>
                            DjikstraHelper.GetTotalCost(_simulationState.Grid, t.RemainingPath.ToList()));
                        if (amountCoins > remindPathCost+1200)
                        {
                            var path = DjikstraHelper.GetShortestPath(_simulationState.Grid, order.OriginNodeId,
                                order.TargetNodeId);
                            var cost = DjikstraHelper.GetTotalCost(_simulationState.Grid, path);
                            var time = DjikstraHelper.GetTotalTime(_simulationState.Grid, path);
                            
                            if (cost < order.Value && order.ExpirationDateUtc > DateTime.Now + time)
                            {
                                if ((await orderService.GetAllOrders()).Find(o => o.Id == order.Id) == null) continue;
                                await orderService.AcceptOrder(order.Id);
                                var transporterId = await transporterService.Buy(order.OriginNodeId);

                                var pathQueue = new Queue<int>(path);
                                pathQueue.Dequeue();
                                _simulationState.Transporters.Add(new TransporterInfo
                                    { Id = transporterId, Orders = [order], RemainingPath = pathQueue });
                            }
                        }
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
            
            if (transporter.RemainingPath.Count > 0)
            {
                var targetNode = transporter.RemainingPath.Dequeue();
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