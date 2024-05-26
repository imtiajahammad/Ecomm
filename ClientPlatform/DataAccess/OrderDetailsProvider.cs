using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClientPlatform.Models;

namespace ClientPlatform.DataAccess
{
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
}

