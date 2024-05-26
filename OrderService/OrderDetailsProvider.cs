using System;
using System.Data.SqlClient;
using Dapper;

namespace OrderService
{
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
}

