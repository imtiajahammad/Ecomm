using System;
namespace ClientPlatform.DataAccess
{
	public class OrderDetailsProvider
	{
        private readonly IHttpClientFactory _httpClientFactory;

        public OrderDetailsProvider(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        /*public OrderDetail[] Get()
        {
            using var client = _httpClientFactory.CreateClient("order");
        }*/
    }
}

