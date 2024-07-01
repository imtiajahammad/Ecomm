// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Producer;

Console.WriteLine("Hello, World!");

var factory = new ConnectionFactory
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672")
    };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
//QueueProducer.Publish(channel);
//DirectExchangePublisher.Publish(channel);
//TopicExchangeProducer.Publish(channel);
HeaderExchangeProducer.Publish(channel);