using System;
using System.Text.Json.Serialization;
namespace OrderService
{
	public class OrderDetail
	{

        [JsonPropertyName("User")]
        public string User { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
}

