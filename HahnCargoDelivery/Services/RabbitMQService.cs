using System.Text;
using HahnCargoDelivery.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HahnCargoDelivery.Services;

public interface IRabbitMQService
{
    public IConnection GetConnection();
    public IModel GetChannel();
}
public class RabbitMQService: IRabbitMQService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMQService()
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

    public IConnection GetConnection() => _connection;
    public IModel GetChannel() => _channel;

}