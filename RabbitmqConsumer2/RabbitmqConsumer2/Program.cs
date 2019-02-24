using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitmqConsumer2
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
                VirtualHost = "/dev",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var exchange = "exchange-direct-week";
                var queue = "ReportWeekSubscribe";
                var key = "routing-ReportSubscribe-week-key";
                var consumerKey = "routing-delay-week";
                var send = "ReportSend";

                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: queue,
                    exchange: exchange,
                    routingKey: consumerKey);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

                Console.WriteLine(" [*] Waiting for messages.  2");

                var r = new Random();
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var data = JsonConvert.DeserializeObject<SubscribeInfo>(message);
                    var routingKey = ea.RoutingKey;

                    var num = r.Next(1, 100);
                    if (data.Type!=SubscribeType.Week && data.Type != SubscribeType.Init)
                    {
                        Console.WriteLine(" [x]不该我接收的， Received '{0}':'{1}':{2}",
                            routingKey, message, num);
                    }
                    else
                    {
                        Console.WriteLine(" [x]Week Received '{0}':'{1}':{2}",
                            routingKey, message, num);
                    }
                    if (num % 5 == 0)
                    {
                        using (var connection3 = factory.CreateConnection())
                        {
                            using (var channel3 = connection3.CreateModel())
                            {
                                var lastWeek = DateTime.Now.AddDays(-7);
                                var date = int.Parse(lastWeek.ToString("yyyyMMdd"));
                                channel3.QueueDeclare(queue: send, durable: true, exclusive: false, autoDelete: false, arguments: null);
                                channel3.BasicPublish(exchange: "", routingKey: send, basicProperties: null,
                                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SendInfo
                                    {
                                        SubscribeId = "Week" + num,
                                        TeamId = "Week" + num,
                                        StartDate = date + 1 - Convert.ToInt32(lastWeek.DayOfWeek.ToString("d")),
                                        EndDate = date + 6
                                    })));
                            }
                        }
                    }

                    using (var connection2 = factory.CreateConnection())
                    using (var channel2 = connection2.CreateModel())
                    {
                        var info = new SubscribeInfo
                        {
                            Type = SubscribeType.Week,
                            UserId = string.Empty
                        };
                        var newBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info));

                        var properties = channel2.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        properties.Expiration = (10 * 1000).ToString();
                        channel2.BasicPublish(exchange: exchange,
                            routingKey: key,
                            basicProperties: properties,
                            body: newBody);
                    }

                };
                channel.BasicConsume(queue: queue,
                    autoAck: true,
                    consumer: consumer);

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
