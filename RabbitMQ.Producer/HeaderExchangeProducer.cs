using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitMQ.Producer;

public static class HeaderExchangeProducer
{
    public static void Publish(IModel channel)
    {
        var ttl = new Dictionary<string, Object>
        {
            { "x-message-ttl", 30000 }
        };
        channel.ExchangeDeclare("demo-header-exchange",ExchangeType.Headers, arguments: ttl);
        
        var count = 0;
        while(true)
        {
            var message = new { Name = "Producer", Message = $"Hello! Count:{count}" };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, Object> { { "account", "new" } };

            channel.BasicPublish("demo-header-exchange", string.Empty, properties, body);
            count++;
            Thread.Sleep(1000);
        }
    }
}
