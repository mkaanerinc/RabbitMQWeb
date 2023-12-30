﻿using RabbitMQ.Client;

namespace RabbitMQWeb.Watermark.Services
{
    public class RabbitMQClientService :IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWatermark = "watermark-route-image";
        public static string QueueName = "queue-watermak-image";

        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, 
            ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            Connect();
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();

            if(_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName,"direct",true,false);

            _channel.QueueDeclare(QueueName,true,false,false,null);

            _channel.QueueBind(QueueName,ExchangeName,RoutingWatermark);

            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");

            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile bağlantı koptu...");
        }
    }
}