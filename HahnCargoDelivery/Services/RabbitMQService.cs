using System.Text;
using HahnCargoDelivery.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HahnCargoDelivery.Services;

public interface IRabbitMqService
{
    public void InitRabbitMqListener();
    public Task StartRabbitMqListener(Func<Order, Task> orderHandler, CancellationToken stoppingToken);
    public void DestroyRabbitMqListener();
}
public class RabbitMqService: BackgroundService, IRabbitMqService
{
    private readonly ILogger<RabbitMqService> _logger;
    private IConnection _connection;
    private IModel _channel;
    private Func<Order, Task>? _orderHandler;

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
        // Set the QoS to 1 to process one message at a time
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
    }

    public async Task StartRabbitMqListener(Func<Order, Task> orderHandler, CancellationToken stoppingToken)
    {
        _orderHandler = orderHandler;
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
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            _logger.LogInformation($"Received message: {message}");

            var order = JsonConvert.DeserializeObject<Order>(message);
            if (order == null)
            {
                throw new Exception("Order can't be null");
            }
            _logger.LogInformation($"Received message(DateTime): {order.ExpirationDateUtc}");
            if(_orderHandler != null)
                await _orderHandler(order);
            
            // Acknowledge the message only after processing is complete
            _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        };

        _channel.BasicConsume(queue: "HahnCargoSim_NewOrders",
            autoAck: false,
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