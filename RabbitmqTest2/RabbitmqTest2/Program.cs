using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitmqTest2
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost",
                UserName = "sanchez-dev",
                Password = "123456",
                VirtualHost = "/dev"
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                for (int i = 0; i < 1; i++)
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;
                    properties.Expiration = (i * 1000).ToString();

                    var exchange = "exchange-direct-week";
                    var queue = "ReportSubscribeWeek";
                    var key = "routing-ReportSubscribe-week-key";
                    var consumerKey = "routing-delay-week";
                    channel.ExchangeDeclare(exchange: exchange,
                        type: "direct");

                    var dic = new Dictionary<string, object>
                    {
                        {"x-dead-letter-exchange", exchange},//过期消息转向路由
                        {"x-dead-letter-routing-key", consumerKey}//过期消息转向路由相匹配routingkey
                    };
                    channel.QueueDeclare(queue, true, false, false, dic);
                    channel.QueueBind(queue, exchange, key);
                    //var message = "Hello World!" + i;
                    //var body = Encoding.UTF8.GetBytes(message);
                    var message = new SubscribeInfo
                    {
                        Type = SubscribeType.Init,
                        UserId = string.Empty
                    };
                    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
                    channel.BasicPublish(exchange: exchange,
                        routingKey: key,
                        basicProperties: properties,
                        body: body);
                    Console.WriteLine(" [x] Sent '{0}':'{1}'", queue, message);
                }
            }

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
