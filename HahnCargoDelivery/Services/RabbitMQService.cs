using System.Text;
using HahnCargoDelivery.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HahnCargoDelivery.Services;

public interface IRabbitMqService
{
    public void InitRabbitMqListener();
    public Task StartRabbitMqListener(Action<Order> handleOrderAction, CancellationToken stoppingToken);
    public void DestroyRabbitMqListener();
}
public class RabbitMqService: BackgroundService, IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection _connection;
    private IModel _channel;
    private Action<Order>? _handleOrderAction;

    public RabbitMqService(ILogger<RabbitMqService> logger)
    {
        InitRabbitMqListener();
        _logger = logger;
    }
    
    public void InitRabbitMqListener()
    {
        var factory = new ConnectionFactory { HostName = "localhost" };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: "HahnCargoSim_NewOrders",
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public async Task StartRabbitMqListener(Action<Order> handleOrderAction, CancellationToken stoppingToken)
    {
        _handleOrderAction = handleOrderAction;
        await ExecuteAsync(stoppingToken);
    }
    
    public void DestroyRabbitMqListener()
    {
        Dispose();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Received message: {message}");

            var order = JsonConvert.DeserializeObject<Order>(message);
            if (order == null)
            {
                throw new Exception("Order can't be null");
            }

            _handleOrderAction?.Invoke(order);
        };

        _channel.BasicConsume(queue: "HahnCargoSim_NewOrders",
            autoAck: true,
            consumer: consumer);

        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }

}