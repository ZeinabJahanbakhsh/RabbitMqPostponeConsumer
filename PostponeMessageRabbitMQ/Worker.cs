using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PostponeMessageRabbitMQ.Config;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PostponeMessageRabbitMQ
{
    public class Worker : BackgroundService
    {
        private IModel _channel;
        private IConnection _connection;
        private readonly ILogger<Worker> _logger;
        private readonly RabbitConfig _options;
        private readonly RabbitQueueConfig _queueConfig;
        private ConnectionFactory factory;
        

        public Worker(ILogger<Worker> logger, RabbitConfig options, RabbitQueueConfig queueConfig)
        {
            _logger = logger;
            _options = options;
            _queueConfig = queueConfig;
            InitRabbitMq();
        }

        private void InitRabbitMq()
        {
            factory = new ConnectionFactory
            {
                Port = _options.Port,
                HostName = _options.HostName,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VHost,
                DispatchConsumersAsync =true
            };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            FetchChannel(_channel);
            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            _logger.LogError("connection is down");
        }

        public void FetchChannel(IModel channel)
        {
            // CreateMain Channels
            channel.ExchangeDeclare(_queueConfig.ExchangeName, ExchangeType.Topic, true);
            channel.QueueDeclare(_queueConfig.QueueName, false, false, false, null);
            channel.QueueBind(_queueConfig.QueueName, _queueConfig.ExchangeName, _queueConfig.QueueRouteKey, null);
            
            //Create Postpone Channels
            var dic = new Dictionary<string, object> { { "x-dead-letter-exchange", _queueConfig.ExchangeName } };
            channel.ExchangeDeclare(_queueConfig.DelayExchangeName, ExchangeType.Topic, true, false, dic);
            channel.QueueDeclare(_queueConfig.DelayQueueName, false, false, false, dic);
            channel.QueueBind(_queueConfig.DelayQueueName, _queueConfig.DelayExchangeName, _queueConfig.QueueRouteKey, null);
            
            //Create Expired Channel to log in system!!
            channel.QueueDeclare(_queueConfig.ExpiredQueueName, false, false, false, null);
            channel.QueueBind(_queueConfig.ExpiredQueueName, _queueConfig.ExchangeName, _queueConfig.ExpiredRouteKey, null);

        }




        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string retryHeader = "RetryCount";
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {

                // received message  
                var content = System.Text.Encoding.UTF8.GetString(ea.Body.ToArray());
                string type = ea.BasicProperties.Type;

                try
                {
                    //DO your Task
                    await HandleMessage(content);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "error in recieve object");

                }

                int tryCounter = 1;
                if (ea.BasicProperties.Headers == null)
                {
                    ea.BasicProperties.Headers = new Dictionary<string, object>();
                }
                else
                {
                    object tryCount = 0;
                    ea.BasicProperties.Headers.TryGetValue(retryHeader, out tryCount);
                    tryCounter = (int?)tryCount ?? 0;
                    ea.BasicProperties.Headers.Remove(retryHeader);
                    ea.BasicProperties.Headers.Add(retryHeader, ++tryCounter);
                }

                if (tryCounter > _queueConfig.TryCount)
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    _channel.BasicPublish(_queueConfig.ExchangeName, _queueConfig.ExpiredRouteKey, ea.BasicProperties,
                        ea.Body);

                }
                else
                {
                    _channel.BasicAck(ea.DeliveryTag, false);
                    ea.BasicProperties.Expiration = (tryCounter * _queueConfig.PostponeRateMiliSecond).ToString();
                    _channel.BasicPublish(_queueConfig.DelayExchangeName, _queueConfig.QueueRouteKey, ea.BasicProperties,
                        ea.Body);
                }

            };
            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(_queueConfig.QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private Task OnConsumerRegistered(object sender, ConsumerEventArgs @event)
        {
            return Task.CompletedTask;
        }

        private Task OnConsumerUnregistered(object sender, ConsumerEventArgs @event)
        {
            return Task.CompletedTask;
        }

        private Task OnConsumerConsumerCancelled(object sender, ConsumerEventArgs @event)
        {
            return Task.CompletedTask;
        }

        private Task OnConsumerShutdown(object sender, ShutdownEventArgs @event)
        {
            _logger.LogError("Consumer is down");
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }

        public async Task HandleMessage(string message)
        {
            throw new NotImplementedException();
        }



    }
}
