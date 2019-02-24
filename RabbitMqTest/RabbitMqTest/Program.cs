using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace RabbitMqTest
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

                    var exchange = "exchange-direct";
                    var queue = "ReportSubscribe";
                    var key = "routing-ReportSubscribe-key";
                    var consumerKey = "routing-delay";
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

            //using (var connection = factory.CreateConnection())
            //using (var channel = connection.CreateModel())
            //{
            //    for (var i = 0; i < 1; i++)
            //    {
            //        var properties = channel.CreateBasicProperties();
            //        properties.DeliveryMode = 2;
            //        properties.Expiration = (i * 1000).ToString();

            //        var exchange = "xxxx";
            //        var queue = "yyyy";
            //        var key = "zzzz";
            //        var consumerKey = "zzzz-consumer";
            //        channel.ExchangeDeclare(exchange: exchange,
            //            type: "direct");

            //        var dic = new Dictionary<string, object>
            //        {
            //            {"x-dead-letter-exchange", exchange},//过期消息转向路由
            //            {"x-dead-letter-routing-key", consumerKey}//过期消息转向路由相匹配routingkey
            //        };
            //        channel.QueueDeclare(queue, true, false, false, dic);
            //        channel.QueueBind(queue, exchange, key);
            //        var message = "Hello World!" + i;
            //        var body = Encoding.UTF8.GetBytes(message);
            //        channel.BasicPublish(exchange: exchange,
            //            routingKey: key,
            //            basicProperties: properties,
            //            body: body);
            //        Console.WriteLine(" [x] Sent '{0}':'{1}'", queue, message);
            //    }
            //}

            //Console.WriteLine(" Press [enter] to exit.");
            //Console.ReadLine();



            // *** 执行不了的rabbitmq延迟队列网络demo  ***
            //using (var connection = factory.CreateConnection())
            //{
            //    Console.WriteLine("1111");
            //    while (Console.ReadLine() != null)
            //    {
            //        using (var channel = connection.CreateModel())
            //        {

            //            Dictionary<string, object> dic = new Dictionary<string, object>();
            //            dic.Add("x-expires", 30000);
            //            dic.Add("x-message-ttl", 12000);//队列上消息过期时间，应小于队列过期时间  
            //            dic.Add("x-dead-letter-exchange", "exchange-direct");//过期消息转向路由  
            //            dic.Add("x-dead-letter-routing-key", "routing-delay");//过期消息转向路由相匹配routingkey  
            //            //创建一个名叫"zzhello"的消息队列
            //            channel.QueueDeclare(queue: "zzhello",
            //                durable: true,
            //                exclusive: false,
            //                autoDelete: false,
            //                arguments: dic);

            //            var message = "Hello World!";
            //            var body = Encoding.UTF8.GetBytes(message);

            //            //向该消息队列发送消息message
            //            channel.BasicPublish(exchange: "",
            //                routingKey: "zzhello",
            //                basicProperties: null,
            //                body: body);
            //            Console.WriteLine(" [x] Sent {0}", message);
            //        }
            //    }
            //}

            //Console.ReadKey();
        }
    }
}
