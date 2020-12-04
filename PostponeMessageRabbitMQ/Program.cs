using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PostponeMessageRabbitMQ.Config;

namespace PostponeMessageRabbitMQ
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    IConfiguration configuration = hostContext.Configuration;

                    RabbitConfig options = configuration.GetSection("Rabbit")
                                                        .Get<RabbitConfig>();


                    RabbitQueueConfig queueConfig = configuration.GetSection("QueueConfig")
                        .Get<RabbitQueueConfig>();
                    services.AddSingleton(options);
                    services.AddSingleton(queueConfig);
                    services.AddHostedService<Worker>();
                });
    }
}
