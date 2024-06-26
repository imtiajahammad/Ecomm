
Microservices:

* What is Monolith?
	- A monolithic application  is an application where every part of a product is part of same service or web application
	- And usually the data for entire application is in a single data store 
* Issues with Monolithic Application
	- Source code management in source control (Merge nightmares)
	- Extremely big team, hence management nightmare
	- Code and Database deployments and rollbacks are a nightmare.
* What is Microservice?
	- Microservices are smaller single responsibility services
	- It does one thing and one thing only
	- It has clear boundary
	- Usually they own their data and the data store
	- The logic and data of a single responsibility microservice should not leak outside of it
* What about the cost of deployment?
	- Cost with IIS
		- If all services are deployed in same Windows machine in an IIS, if IIS has an issue all app will fail
		- If you want multiple IIS and boxes, there is significantly higher cost associated with it
	- Cost of Database
		- Having so many databases, especially if we are using SQL server, it is not at all cost effective
		- It is important to have databases isolated from each other
* What is Containers?
	- Containers are used for packaging software and all its dependencies into a standard unit for development, deployment and shipment
	- They are immutable
	- Containers are defined as images, which becomes containers on runtime
	- Docker is the most popular container implementation 
* Difference between containers and virtual machines?
	- Containers
		- Lighweight (in MB sizes)
		- Faster to start
		- Best suited for Microservices
		- Resource utilization can be optimized to use most out of a machine
		- Less secure across containers
	- Virtual Machines
		- Heavy (can go in GB sizes)
		- Slower to start
		- Can be used, but not cost effective
		- Suboptimal resource optimization
		- Secure across VMs
* How is containers helping me?
	- .Net Core applications can run on containers. In fact, .Net Core was built with containers in mind
	- Running in containers gives all the advantages of containers, including savings in cost and efficiency
	- Database like Postgres can be easily run in containers and have isolations we want
* When should I use Microservice?
	- Start with a monolithic application if you are not sure
	- The main decider of going to microservice is your agility of deployment. 
	- Think about which functionality makes sense breaking up for ease of deployment
	- If you are going with microservices, Docker is your best friend
* Tools needed
	- Docker for packaging and deployment
	- When you use Docker, there comes the question of how to scale out or scale down. Hence you need an orchestration engine, Kubernetes is the most popular and open source container orchestration engine
	- Orchestration engines are not necessary to start
* Microservice Communication?
	- Most popular is REST API. (GRPC is catching up quickly)
	- Message based communication (Queue or Pub/Sub), which works very well when you want to offload work to somme background microservices. And the response is not time sensitive.
* How do I convert my monolith to Microservices?
	- Bring out the most easily separable component from the Monolith first
	- If you using SQL Server (alike), your first option might be to make a service responsible for table set instead of an entire database
	- If you are in cloud, use cloud native managed databases like DynamoDB in AWS or Cosmos DB in Azure
* What about reporst?
	- When data is available across multiple databases, getting a consolidated view of the data or report is hard.
	- You can either use data aggregation by calling multiple services
	- Or you can create a data stream for all the data and have an aggregator create a read-only view of the data
* Some Important notes
	- When you start building your microservices, think about resilience during design
	- Establish the communication pattern you have to use between your microservices based on your response needs.
	- Logging and monitoring can not be an afterthought. Cloud based centralised logging is extremely critical. Also passing a correlation log id across microservices would save you lot of debug time in future
	- Don’t build a distributed monolith. Meaning, keep complete decouple between the microservices. 
---
1. Create a blank solution in Visual Studio
2. Add a new API project **OrderService** with **net6.0** framework in the solution
3. Delete the existing Controller **WeatherForecastController** controller and **WeatherForecast** class
4. Create a new controller **OrderController** with read/write action
```
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OrderService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController : Controller
    {
        private readonly IOrderDetailsProvider _orderDetailsProvider;

        public OrderController(IOrderDetailsProvider orderDetailsProvider)
        {
            _orderDetailsProvider = orderDetailsProvider;
        }
        // GET: api/values
        [HttpGet]
        public IEnumerable<OrderDetail> Get()
        {
            return _orderDetailsProvider.Get();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}


```
5. Create new interface for order with the name **IOrderDetailsProvider**
```
public interface IOrderDetailsProvider
{
	OrderDetail[] Get();
}
```
6. Create new class named **OrderDetailsProvider** and implement the **IOrderDetailsProvider**
```
	public class OrderDetailsProvider : IOrderDetailsProvider
	{
		private readonly string _connectionString;
		public OrderDetailsProvider(string connectionString)
		{
			_connectionString = connectionString;
		}
		public OrderDetail[] Get()
		{
			using var connection = new SqlConnection(_connectionString);
			return connection.Query<OrderDetail>(@"SELECT o.UserName AS [User], od.ProductName AS Name, od.Quantity  FROM [Order] o
                                            JOIN [OrderDetail] od on o.Id = od.OrderId")
				.ToArray();
		}
	}
```
7. Create new class **OrderDetail** for model class
```
public class OrderDetail
{
	public string User { get; set; }
	public string Name { get; set; }
	public int Quantity { get; set; }
}
```
8. Install **dapper** package for dbconnection
9. Install **System.Data.SqlClient** for SqlConnection
10. Add connectionString into **appSettings.json**
```
"ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=ecom;User=sa;Password=Docker@123;"
}
```
11. Add the injection in the program file for **IOrderDetailsProvider**
```
var connectionString = builder.Configuration["ConnectionStrings"];
var connectionString2 = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSingleton<IOrderDetailsProvider>(sp => new OrderDetailsProvider(connectionString2));
```
12. Go to the controllers and update the **OrderController** 
```
private readonly OrderDetailsProvider _orderDetailsProvider;

public OrderController(OrderDetailsProvider orderDetailsProvider)
{
        _orderDetailsProvider = orderDetailsProvider;
}
// GET: api/values
[HttpGet]
public IEnumerable<OrderDetail> Get()
}
return _orderDetailsProvider.Get();
}
```
13. Let’s create a database named **ecom** and some tables with some data so that the api can fetch some data
```
CREATE TABLE [dbo].[Inventory] (
    [Id]        INT           IDENTITY (1, 1) NOT NULL,
    [Name]      NVARCHAR (50) NULL,
    [Quantity]  INT           NOT NULL,
    [ProductId] INT           NULL,
    CONSTRAINT [PK_Inventory] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Inventory_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id])
);
```
```
CREATE TABLE [dbo].[Order] (
    [Id]          INT      IDENTITY (1, 1) NOT NULL,
    [UserId]      INT      NOT NULL,
    [UpdatedTime] DATETIME NOT NULL,
    CONSTRAINT [PK_Order] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Order_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[User] ([Id])
);
```
```
CREATE TABLE [dbo].[OrderDetails] (
    [OrderId]   INT NOT NULL,
    [ProductId] INT NOT NULL,
    [Quantity]  INT NULL,
    CONSTRAINT [FK_OrderDetails_Order] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Order] ([Id]),
    CONSTRAINT [FK_OrderDetails_Product] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Product] ([Id])
);
```
```
CREATE TABLE [dbo].[Product] (
    [Id]          INT            IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (50)  NOT NULL,
    [Description] NVARCHAR (MAX) NULL,
    [Type]        NVARCHAR (50)  NOT NULL,
    CONSTRAINT [PK_Product] PRIMARY KEY CLUSTERED ([Id] ASC)
);
```
```
CREATE TABLE [dbo].[User] (
    [Id]      INT            IDENTITY (1, 1) NOT NULL,
    [Name]    NVARCHAR (50)  NOT NULL,
    [Address] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([Id] ASC)
);
```
14. Now let’s run the **orderservice** and test the get API
15. Add a **README.md** file in the solution directory
```
touch README.md
```
16.   Add a **gitignore** file in the solution directory
```
dotnet new gitignore
```
---
17. Now lets create a **MVC** applicaton where we can consume the **OrderService**. Lets Create a **MVC** project with **dotnet6.0** named **ClientPlatform**
18. Add a new folder **DataAccess** and add a new class called **OrderDetailsProvider**
```
	public class OrderDetailsProvider : IOrderDetailsProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OrderDetailsProvider> _logger;


        public OrderDetailsProvider(IHttpClientFactory httpClientFactory, ILogger<OrderDetailsProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }
        public async Task<OrderDetail[]> Get()
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("order");
                var response = await client.GetAsync("/api/order");
                var data = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<OrderDetail[]>(data);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Error getting order detials, ex:{ex}");
                return Array.Empty<OrderDetail>();
            }
        }
    }
```
19. Create a folder for models **Models** and add a class **OrderDetail**
```
	public class OrderDetail
	{

        [JsonPropertyName("User")]
        public string User { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
```
20. Add injection for OrderDetailsProvider with HttpClientFacotry in the program file
```
builder.Services.AddSingleton<IOrderDetailsProvider, OrderDetailsProvider>();

builder.Services.AddHttpClient("order", config =>
    config.BaseAddress = new System.Uri("https://localhost:7177/"));
```
21. Add a new class called **OrderDetailsProvider** in **DataAccess** folder
```
public interface IOrderDetailsProvider
{
    Task<OrderDetail[]> Get();
}
```
22. Add the method for the **HomeController** 
```
public class HomeController : Controller
{
    private readonly IOrderDetailsProvider _orderDetailsProvider;
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, IOrderDetailsProvider orderDetailsProvider)
    {
        _logger = logger;
        _orderDetailsProvider = orderDetailsProvider;
    }

    public async Task<IActionResult> Index()
    {
        var orderDetails = await _orderDetailsProvider.Get();
        return View(orderDetails);
    }
}
```
23. Edit the View **Index.cshtml**
```
@model ClientPlatform.Models.OrderDetail[]
@{
    ViewData["Title"] = "Home Page - Ecomm";
}
<style>
    table {
        font-family: arial, sans-serif;
        border-collapse: collapse;
        width: 100%;
    }

    td, th {
        border: 1px solid #dddddd;
        text-align: left;
        padding: 8px;
    }

    tr:nth-child(even) {
        background-color: #dddddd;
    }
</style>
<div class="text-center">
    <h1 class="display-4">Welcome</h1>
    <p>Learn about <a href="https://docs.microsoft.com/aspnet/core">building Web apps with ASP.NET Core</a>.</p>
</div>

<div class="text-center">
    <table>
        <thead>
            <tr >
                <th>User Name</th>
                <th>Product Name</th>
                <th>Quantity</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var item in Model)
            {
                <tr >
                    <td>@item.User </td>
                    <td>@item.Name </td>
                    <td>@item.Quantity</td>
                </tr>
            }
        </tbody>
    </table>
</div>
```
24. If you check the query of fething the orderDetails, you might find that there is a joining with the table **Product** which is not recommended. We can avoid this scenario by adding a new column into **OrderDetails** table called **Name** and remove the joining with the **Product** table.
```
ALTER TABLE [dbo].[OrderDetails]
    ADD [ProductName] NVARCHAR (50) NULL;
```
Also edit the existing data and add productNames.   
Update the query in **OrderService** in **OrderDetailsProvider**
```
public class OrderDetailsProvider : IOrderDetailsProvider
    {
        private readonly string _connectionString;
        public OrderDetailsProvider(string connectionString)
        {
            _connectionString = connectionString;
        }
        public OrderDetail[] Get()
        {
            using var connection = new SqlConnection(_connectionString);

           return connection.Query<OrderDetail>(@"
                                                    SELECT u.Name AS [USER], od.ProductName AS Name, od.Quantity    from [Order] o
													JOIN [OrderDetails] od ON o.Id = od.OrderId
													JOIN [User] u ON o.UserId = u.Id"
            )
                .ToArray();
        }
    }
```
25. Similarly, with 24, fething **Orders** should not have a joining query with the **User** table. Instead we can add a new column with **UserName** to avoid this scenario
```
ALTER TABLE [dbo].[Order]
    ADD [UserName] NVARCHAR (50) NULL;
```
Also edit the **Order** table and add the **UserNames** in the table.
And update the query
```
	public class OrderDetailsProvider : IOrderDetailsProvider
    {
        private readonly string _connectionString;
        public OrderDetailsProvider(string connectionString)
        {
            _connectionString = connectionString;
        }
        public OrderDetail[] Get()
        {
            using var connection = new SqlConnection(_connectionString);

           return connection.Query<OrderDetail>(@"SELECT o.UserName AS [USER], od.ProductName AS Name, od.Quantity from [Order] o
													JOIN [OrderDetails] od ON o.Id = od.OrderId"
            )
                .ToArray();
        }
    }
```
---
RabbitMQ:   
- What is a Message Broker
    - You can think of a message broker like a post office. 
    - Its main responsibility is to broker messages between publisher and subscribers
    - Once a message is received by a message broker from a producer, it routes the message to a subscriber. 
    - Message broker pattern is one of the most useful pattern when it comes to ddecoupling microservices.
    - **Producer**: An application responsible for sending message
    - **Consumer**: An application listening for messages.
    - **Queue**: Storage where messages are stored by the broker.
- What is RabbitMQ
    - RabbitMQ is an open source message broker.
    - It is probably one of the most widely used message broker out there
    - RabbitMQ is extremely lightweight and very easy to deploy
    - RabbitMQ supports multiple protocols
    - RabbitMQ is highly available and scalable
    - RabbitMQ supports multiple operating systems
- Protocols Supported:
    - **AMQP 0-9-1**: a binary messaging protocol specification. This is the core protocol specification implemented in RabbitMQ. All other protocol support in RabbitMQ is through Plugins
    - **STOMP**: A text based message protocol
    - **MQTT**: Binary protocol focusing mainly on Publish/Subscribe scenarios
    - **AMQP 1.0**
    - **HTTP and WebSocket**

26. Let's create a console application **RabbitMQ.Producer** in the same solution
27. Now, let't create a docker image for **RabbitMQ** and open up the browser for ***http://localhost:15672/#/** with **guest** username and **guest** password
```
docker images
docker run -d --hostname my-rabbit --name ecomm-rabbit -p 15672:15672 -p 5672:5672 rabbitmq:3-management
docker logs -f e67
```
28. Search for the package **RabbitMQ.Client** and install on **RabbitMQ.Producer**
29. Now add the following codes in **Program.cs**
```
var factory = new ConnectionFactory
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672")
    };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
channel.QueueDeclare("demo-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);
var message = new { Name = "Producer", Message = "Hello!" };
var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

channel.BasicPublish("", "demo-queue", null, body);
```
For using JsonConvert.SerializeObject, install the package Newtonsoft.json
30. Now create another console application named **RabbitMQ.Consumer** on the same solution. Also add the package **RabbitMQ.Client**
31. Now add the following codes into **Program.cs**
```
var factory = new ConnectionFactory
{
    Uri = new Uri("amqp://guest:guest@localhost:5672")
};
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();
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
Console.ReadLine();
```
32. Now run the consumer first, it will create the queue in the RabbitMQ server; then run the producer, it will send a message into the queue and you can see it coming in the queue. And finally run the consumer again, you will see the message is received and being print in the console according to the code.
---
33. Now go to **RabbitMQ.Producer**, open up **program.cs** file and take the following code from that class
    ```
    channel.QueueDeclare("demo-queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
    var message = new { Name = "Producer", Message = "Hello!" };
    var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

    channel.BasicPublish("", "demo-queue", null, body);
    ```
And now create a new static class **QueueProducer.cs** and place the code here into a static funciton
    ```
    public static void Publish(IModel channel)
    {
        channel.QueueDeclare("demo-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);
        var message = new { Name = "Producer", Message = "Hello!" };
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

        channel.BasicPublish("", "demo-queue", null, body);
    }
    ```
Now call that function from **program.cs** file by the following code
    ```
    var factory = new ConnectionFactory
    {
        Uri = new Uri("amqp://guest:guest@localhost:5672")
    };
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();
    QueueProducer.Publish(channel);
    ```
34. To publish multiple messages at the same time, let's modify the **QueueProducer.cs**
    ```
    public static class QueueProducer
    {
        public static void Publish(IModel channel)
        {
            channel.QueueDeclare("demo-queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
            
            var count = 0;
            while(true)
            {
                var message = new { Name = "Producer", Message = $"Hello! Count:{count}" };
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish("", "demo-queue", null, body);
                count++;
                Thread.Sleep(1000);
            }

        }
    }    
    ```
35. Let's go to **RabbitMQ.Consumer** and open up the **program.cs** file and take the following code portion
    ```
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
        Console.ReadLine();
    ```
    And create a new class **QueueConsumer.cs** and add the taken code into the following static method
    ```
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
    ```
    And call the method **Consume** from **program.cs** class
    ```
    QueueConsumer.Consume(channel);
    ```
36. Now let's run the **RabbitMQ.Consumer** from two instances, and run the **RabbitMQ.Producer** then. You will notice that one instance of the consumer is getting the even messges and the other one is getting the odd messges. 
So, if we have multiple consumers to a single queue, the messages will be evenly distributed across the consumers.
RabbitMQ gives us the option to scale the service by providing the horizontal distribution. It also ensures 1 consumer gets unique message, 1 message does not go to multiple consumers.

RabbitMQ:   
- What is an Exchange?
    - Exchanges are exactly the name suggests
    - The are exchanges for message
    - Just like a stock exchange, where people exchanges stocks, a seller sells stocks to a buyer. And exchange acts as a router of the stocks
    - Similarly, Exchanges in RabbitMQ routes messages from a producer to a single consumer or multiple consumers
    - An exchange uses **header attributes**, **routing keys** and **binding** to route messages
    - In RabbitMQ, infact, messages are never published to a queue, they always goes through an Exchange. Event when we send message to a queue it uses default exchange. **[Exchange:(AMQP default)]**
Types of Exchange:
    - **Direct**: Direct exchange uses **routing key** in the header to identify which queue the message should be sent to. Routing key is a **header value** set by the **producer**. And consumers uses the **routing key** to **bind** to the **queue**. The exchange does **exact match** of routing key values.
    - **Topic**: This is kind of **similar** to Direct. Topic exchange also uses **routing key**, but it does not do an **exact match** on the routing key, instead it does a **pattern match** based on pattern.
    - **Header**: Header excahnge routes messages based on **header values** and are very similar to Topic exchange
    - **Fanout**: As the name suggests, Fanout exchange routes messages to all the queues bound to it. It routes all the messages to all the queues, it does not look into the routing keys or anything else.

37. Now, let's explore the **Direct Exchange**. Let's go to **RabbitMQ.Producer** and create a class named **DirectExchangePublisher** and add the following code.
    ```
    public static class DirectExchangePublisher
    {
        public static void Publish(IModel channel)
        {
            channel.ExchangeDeclare("demo-direct-exchange",ExchangeType.Direct);
            
            var count = 0;
            while(true)
            {
                var message = new { Name = "Producer", Message = $"Hello! Count:{count}" };
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish("demo-direct-exchange", "account-init", null, body);
                count++;
                Thread.Sleep(1000);
            }
        }
    }
    ```
Also, modify the **program.cs** and call **DirectExchangePublisher** instead of **QueueProducer**
    ```
    //QueueProducer.Publish(channel);
    DirectExchangePublisher.Publish(channel);
    ```
38. Now let's do the same thing for **RabbitMQ.Consumer** and create a new class called **DirectExchangeConsumer.cs** and add the following code.
    ```
    public static class DirectExchangeConsumer
    {
        public static void Consume(IModel channel)
        {
            channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);
            channel.QueueDeclare("demo-direct-queue",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);
            channel.QueueBind("demo-direct-queue","demo-direct-exchange","account-init");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, e) =>
            {
                var body = e.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($"{message}");
            };

            channel.BasicConsume("demo-direct-queue", true, consumer);
            Console.WriteLine("Consumer Started");
            Console.ReadLine();
        }
    }
    ```
Also modify the **program.cs** to call the **DirectExchangeConsumer** method

39. Now let's run the consumer and check out the RabbitMQ server. You will find out your exchange **demo-direct-exchange** and exchange-queue **demo-direct-queue** in the server. And if you check out the exchange bindings, you will find out the exhange is binded to the queue with routing-key **account-init**
Now run the **RabbitMQ.Producer** and check out messages coming from your defined exhange.

40. Now, we will explore 2 more things, one is **Lifetime of a message** and the other one is **Prefetch count**. These are 2 important concepts for queue.
**Prefetch Count**: When we have multiple consumers connected to a queue, then **Prefetch count** tell how many messages that perticular consumer can prefetch and process. If prefetch count is 2, if there are 10 messages to the queue, every consumer will take only 2 message and those 2 will be delivered only.
Now let's implement these 2 concepts and start with implementing the **Lifetime of a message** by modifying the **DirectExchangePublisher.cs** in the **RabbitMQ.Producer**
```
    public static class DirectExchangePublisher
    {
        public static void Publish(IModel channel)
        {
            var ttl = new Dictionary<string, Object>
            {
                { "x-message-ttl", 30000 }
            };
            channel.ExchangeDeclare("demo-direct-exchange",ExchangeType.Direct, arguments: ttl);
            
            var count = 0;
            while(true)
            {
                var message = new { Name = "Producer", Message = $"Hello! Count:{count}" };
                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                channel.BasicPublish("demo-direct-exchange", "account-init", null, body);
                count++;
                Thread.Sleep(1000);
            }
        }
    }
```
Now, let's implement the **Prefetch Count** in the **DirectExchangeConsumer.cs** in the **RabbitMQ.Consumer**
```
public static class DirectExchangeConsumer
{
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare("demo-direct-exchange", ExchangeType.Direct);
        channel.QueueDeclare("demo-direct-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);
        channel.QueueBind("demo-direct-queue","demo-direct-exchange","account-init");
        channel.BasicQos(0,10,false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{message}");
        };

        channel.BasicConsume("demo-direct-queue", true, consumer);
        Console.WriteLine("Consumer Started");
        Console.ReadLine();
    }
}
```
Now go to the RabbitMQ dashboard and delete the queue and the exchange. And then run both the consumer and producer, you will see the **Prefetch count** in the queue section and **TTL** in the exchange section.
41. Create a class **TopicExchangeConsumer.cs** in **RabbitMQ.Consumer**
```
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare("demo-topic-exchange", ExchangeType.Topic);
        channel.QueueDeclare("demo-topic-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);
        channel.QueueBind("demo-topic-queue","demo-topic-exchange","account.*");
        channel.BasicQos(0,10,false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{message}");
        };

        channel.BasicConsume("demo-topic-queue", true, consumer);
        Console.WriteLine("Consumer Started");
        Console.ReadLine();
    }
```
Now update the **program.cs** -
```
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
TopicExchangeConsumer.Consume(channel);
```
42. Create a new class **TopicExchangeProducer** in **RabbitMQ.Producer**
```
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

            channel.BasicPublish("demo-topic-exchange", "account.update", null, body);
            count++;
            Thread.Sleep(1000);
        }
    }
```
Now update the **program.cs** -
```
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
TopicExchangeProducer.Publish(channel);
```   

43. Now run both the **producer** and **consumer** and you will see **consumer** is going to receive the messages. Now update the **routing-key** into **account.update** in  **producer** and check if messages are consumed in the consumer or not. Also try with **user.update** as **routing-key** in the **producer** and check if the messages are consumed or not.

44. Create a new class **HeaderExchangeConsumer** in **RabbitMQ.Consumer** 
```
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare("demo-header-exchange", ExchangeType.Headers);
        channel.QueueDeclare("demo-header-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);

        var header = new Dictionary<string, object> {{ "account", "new" }};

        channel.QueueBind("demo-header-queue","demo-header-exchange",string.Empty,header);
        channel.BasicQos(0,10,false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{message}");
        };

        channel.BasicConsume("demo-header-queue", true, consumer);
        Console.WriteLine("Header Consumer Started");
        Console.ReadLine();
    }
```
Also update the **program.cs** file
```
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
HeaderExchangeConsumer.Consume(channel);
```
45. Create a new class **HeaderExchangeProducer** in **RabbitMQ.Producer** 
```
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
```
Also update the **program.cs** file
```
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
``` 
46. Now run both the **producer** and **consumer** and check if **consumer** can consume the messages. But if you change the **header** value in **producer** and run the applications, you will see, **exchange** will get the messages but the **consumer** will not get any messages
47. Create a new class **FanoutExchangeConsumer** in **RabbitMQ.Consumer** 
```
    public static void Consume(IModel channel)
    {
        channel.ExchangeDeclare("demo-fanout-exchange", ExchangeType.Fanout);
        channel.QueueDeclare("demo-fanout-queue",
                      durable: true,
                      exclusive: false,
                      autoDelete: false,
                      arguments: null);


        channel.QueueBind("demo-fanout-queue","demo-fanout-exchange",string.Empty);
        channel.BasicQos(0,10,false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (sender, e) =>
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"{message}");
        };

        channel.BasicConsume("demo-fanout-queue", true, consumer);
        Console.WriteLine("Fanout Consumer Started");
        Console.ReadLine();
    }
```
Also update the **program.cs** -
```
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
```
48. Create a new class **FanoutExhangeProducer** in **RabbitMQ.Producer**
```
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
```
Also update the **program.cs** -
```
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
//HeaderExchangeProducer.Publish(channel);
FanoutExchangeProducer.Publish(channel);
```
49. Now run multiple instances of **consumer** and run 1 instance of **producer**, will get to observe that every **consumer** will get the messages. If you observe that although the **producer** has **header** properties, **consumers** are getting all the messages. Even if you add **routing-key** in  **producer**, **consumers** will still receive the messages
50. 
---
Reference:   
- https://www.youtube.com/watch?v=atJkRk_MwdU&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=1&ab_channel=DotNetCoreCentral  
- https://www.youtube.com/watch?v=3AKqtggkaIA&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=2&ab_channel=DotNetCoreCentral
- https://www.youtube.com/watch?v=w84uFSwulBI&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=3&ab_channel=DotNetCoreCentral
- https://www.youtube.com/watch?v=Cm2psU-zN90&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=4&ab_channel=DotNetCoreCentral
- https://www.youtube.com/watch?v=EtTPtnn6uKE&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=5&ab_channel=DotNetCoreCentral