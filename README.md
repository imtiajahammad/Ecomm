
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
24. 
---
Reference:   
- https://www.youtube.com/watch?v=atJkRk_MwdU&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=1&ab_channel=DotNetCoreCentral  
- https://www.youtube.com/watch?v=3AKqtggkaIA&list=PLXCqSX1D2fd_6bna8uP4-p3Y8wZxyB75G&index=2&ab_channel=DotNetCoreCentral 13.08
