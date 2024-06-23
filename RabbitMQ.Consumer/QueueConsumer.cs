using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMQ.Consumer;

public static class QueueConsumer
{
    public static void Consume(IModel channel)
    {
        channel.QueueDeclare("demo-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);


        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{message}");
        };

        channel.BasicConsume("demo-queue", true, consumer);
        Console.WriteLine("Consumer Started");
        Console.ReadLine();
    }
}
