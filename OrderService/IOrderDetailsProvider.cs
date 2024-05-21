using System;
namespace OrderService
{
	public interface IOrderDetailsProvider
	{
        OrderDetail[] Get();
    }
}

