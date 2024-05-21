using System;
using ClientPlatform.Models;

namespace ClientPlatform.DataAccess
{
	public interface IOrderDetailsProvider
	{
        Task<OrderDetail[]> Get();
    }
}

