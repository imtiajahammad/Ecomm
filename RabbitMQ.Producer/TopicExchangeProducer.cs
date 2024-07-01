﻿using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitMQ.Producer;

public static class TopicExchangeProducer
{
    public static void Publish(IModel channel)
    {
        var ttl = new Dictionary<string, Object>
        {
            { "x-message-ttl", 30000 }
        };
        channel.ExchangeDeclare("demo-topic-exchange",ExchangeType.Topic, arguments: ttl);
        
        var count = 0;
        while(true)
        {
            var message = new { Name = "Producer", Message = $"Hello! Count:{count}" };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            channel.BasicPublish("demo-topic-exchange", "user.update", null, body);
            count++;
            Thread.Sleep(1000);
        }
    }
}
