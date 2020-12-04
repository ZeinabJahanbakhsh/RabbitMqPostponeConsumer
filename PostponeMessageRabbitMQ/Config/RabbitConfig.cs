using System;
using System.Collections.Generic;
using System.Text;

namespace PostponeMessageRabbitMQ.Config
{
    public class RabbitConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string HostName { get; set; }
        public int Port { get; set; }
        public string VHost { get; set; }
    }

    public class RabbitQueueConfig
    {
        public string QueueName { get; set; }
        public string ExchangeName { get; set; }
        public string DelayExchangeName { get; set; }
        public string DelayQueueName { get; set; }
        public string ExpiredQueueName { get; set; }
        public string ExpiredRouteKey { get; set; }
        public int TryCount { get; set; }
        public int PostponeRateMiliSecond { get; set; }
        public string QueueRouteKey { get; set; }
    }
}
