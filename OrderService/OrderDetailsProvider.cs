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
            /*return connection.Query<OrderDetail>(@"SELECT o.UserName AS [User], od.ProductName AS Name, od.Quantity  FROM [Order] o
                                            JOIN [OrderDetail] od on o.Id = od.OrderId"
            )
				.ToArray();*/
            return connection.Query<OrderDetail>(@"SELECT u.Name AS [USER], p.Name AS Name, od.Quantity from [Order] o
													JOIN [OrderDetails] od ON o.Id = od.OrderId
													JOIN Product p on od.ProductId = p.Id
													JOIN [User] u ON o.UserId = u.Id"
            )
                .ToArray();


        }
    }
}

