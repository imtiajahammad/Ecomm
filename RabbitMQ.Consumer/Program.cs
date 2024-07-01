// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Consumer;

Console.WriteLine("Hello, World!");



var factory = new ConnectionFactory
{
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
// QueueConsumer.Consume(channel);
//DirectExchangeConsumer.Consume(channel);
//TopicExchangeConsumer.Consume(channel);
//HeaderExchangeConsumer.Consume(channel);
FanoutExchangeConsumer.Consume(channel);