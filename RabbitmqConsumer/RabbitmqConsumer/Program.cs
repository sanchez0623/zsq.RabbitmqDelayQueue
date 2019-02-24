using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitmqConsumer
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
                var exchange = "exchange-direct";
                var queue = "ReportDaySubscribe";
                var key = "routing-ReportSubscribe-key";
                var consumerKey = "routing-delay";
                var send = "ReportSend";

                channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
                channel.QueueBind(queue: queue,
                    exchange: exchange,
                    routingKey: consumerKey);
                channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);

                Console.WriteLine(" [*] Waiting for messages.  1");

                var r = new Random();
                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body;
                    var message = Encoding.UTF8.GetString(body);
                    var data = JsonConvert.DeserializeObject<SubscribeInfo>(message);
                    var routingKey = ea.RoutingKey;

                    var num = r.Next(1, 100);
                    if (data.Type != SubscribeType.Day && data.Type != SubscribeType.Init)
                    {
                        Console.WriteLine(" [x]不该我接收的， Received '{0}':'{1}':{2}",
                            routingKey, message, num);
                    }
                    else
                    {
                        Console.WriteLine(" [x]Day Received '{0}':'{1}':{2}",
                            routingKey, message, num);
                    }
                    if (num % 5 == 0)
                    {
                        using (var connection3 = factory.CreateConnection())
                        {
                            using (var channel3 = connection3.CreateModel())
                            {
                                var date = int.Parse(DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
                                channel3.QueueDeclare(queue: send, durable: true, exclusive: false, autoDelete: false, arguments: null);
                                channel3.BasicPublish(exchange: "", routingKey: send, basicProperties: null,
                                    body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new SendInfo
                                    {
                                        SubscribeId = "Day" + num,
                                        TeamId = "Day" + num,
                                        StartDate = date,
                                        EndDate = date
                                    })));
                            }
                        }
                    }

                    using (var connection2 = factory.CreateConnection())
                    using (var channel2 = connection2.CreateModel())
                    {
                        var info = new SubscribeInfo
                        {
                            Type = SubscribeType.Day,
                            UserId = string.Empty
                        };
                        var newBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(info));

                        var properties = channel2.CreateBasicProperties();
                        properties.DeliveryMode = 2;
                        properties.Expiration = (20 * 1000).ToString();
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

            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    var exchange = "xxxx";
            //    var queue = "yyyy123";
            //    var key = "zzzz";
            //    var consumerKey = "zzzz-consumer";

            //    channel.QueueDeclare(queue: queue, durable: true, exclusive: false, autoDelete: false, arguments: null);
            //    channel.QueueBind(queue: queue,
            //        exchange: exchange,
            //        routingKey: consumerKey);
            //    channel.BasicQos(prefetchSize: 0, prefetchCount: 100, global: false);
            //    //channel.ExchangeDeclare(exchange: exchange,
            //    //    type: "direct");

            //    Console.WriteLine(" [*] Waiting for messages.");

            //    var consumer = new EventingBasicConsumer(channel);
            //    consumer.Received += (model, ea) =>
            //    {
            //        var body = ea.Body;
            //        var message = Encoding.UTF8.GetString(body);
            //        var routingKey = ea.RoutingKey;
            //        Console.WriteLine(" [x] Received '{0}':'{1}'",
            //            routingKey, message);

            //        using (var connection2 = factory.CreateConnection())
            //        using (var channel2 = connection2.CreateModel())
            //        {
            //            var properties = channel2.CreateBasicProperties();
            //            properties.DeliveryMode = 2;
            //            properties.Expiration = (5 * 1000).ToString();
            //            channel2.BasicPublish(exchange: exchange,
            //                routingKey: key,
            //                basicProperties: properties,
            //                body: body);
            //        }
            //    };
            //    channel.BasicConsume(queue: queue,
            //        autoAck: true,
            //        consumer: consumer);

            //    Console.WriteLine(" Press [enter] to exit.");
            //    Console.ReadLine();
            //}


            // *** 执行不了的rabbitmq延迟队列网络demo  ***
            //using (var connection = factory.CreateConnection())
            //{
            //    Console.WriteLine("2222");
            //    using (var channel = connection.CreateModel())
            //    {
            //        channel.ExchangeDeclare(exchange: "exchange-direct", type: "direct");
            //        string name = channel.QueueDeclare().QueueName;
            //        channel.QueueBind(queue: name, exchange: "exchange-direct", routingKey: "routing-delay");

            //        //回调，当consumer收到消息后会执行该函数
            //        var consumer = new EventingBasicConsumer(channel);
            //        consumer.Received += (model, ea) =>
            //        {
            //            var body = ea.Body;
            //            var message = Encoding.UTF8.GetString(body);
            //            Console.WriteLine(ea.RoutingKey);
            //            Console.WriteLine(" [x] Received {0}", message);
            //        };

            //        //Console.WriteLine("name:" + name);
            //        //消费队列"hello"中的消息
            //        channel.BasicConsume(queue: name,
            //            autoAck: true,
            //            consumer: consumer);

            //        Console.WriteLine(" Press [enter] to exit.");
            //        Console.ReadLine();
            //    }
            //}

            //Console.ReadKey();
        }
    }
}
