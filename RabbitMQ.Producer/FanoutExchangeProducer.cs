using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace RabbitMQ.Producer;

public class FanoutExchangeProducer
{
    public static void Publish(IModel channel)
    {
        var ttl = new Dictionary<string, Object>
        {
            { "x-message-ttl", 30000 }
        };
        channel.ExchangeDeclare("demo-fanout-exchange",ExchangeType.Fanout, arguments: ttl);
        
        var count = 0;
        while(true)
        {
            var message = new { Name = "Producer", Message = $"Hello! Count:{count}" };
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            var properties = channel.CreateBasicProperties();
            properties.Headers = new Dictionary<string, Object> { { "account", "new" } };/* not needed but kept it to prove that fanout will go to every consumer*/

            channel.BasicPublish("demo-fanout-exchange", string.Empty, properties, body);
            count++;
            Thread.Sleep(1000);
        }
    }
}
