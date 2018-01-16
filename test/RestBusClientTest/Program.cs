using Sino.RestBus.Client;
using Sino.RestBus.RabbitMQ;
using Sino.RestBus.RabbitMQ.Client;
using System;
using System.Net.Http;
using System.Text;

namespace RestBusClientTest
{
    class Program
    {
        public const string AMQP_URL = "localhost:5672";
        public const string SERVICE_NAME = "test";

        static void Main(string[] args)
        {
            var msgMapper = new BasicMessageMapper(AMQP_URL, SERVICE_NAME);

            var client = new RestBusClient(msgMapper);
            var options = new RequestOptions();
            options.Headers.Add("Accept", "application/json");

            Console.WriteLine("Sending Message...");

            var res = client.PostAsJsonAsync("/api/test", new { Val = 10 }, options).Result;

            Console.WriteLine("Response Received:\n{0}\nContent:\n{1}\n", res, Encoding.UTF8.GetString((res.Content as ByteArrayContent).ReadAsByteArrayAsync().Result));

            client.Dispose();
        }
    }
}
