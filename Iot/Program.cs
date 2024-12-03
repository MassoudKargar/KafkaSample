using Common;
using Confluent.Kafka;
using System.Text.Json;
class Program
{
    static async Task Main(string[] args)
    {
        // Kafka configuration
        var config = new ProducerConfig
        {
            BootstrapServers = "192.168.88.151:9092",
            AllowAutoCreateTopics = true
        };

        string topic = "Location";

        // Create Kafka producer
        using (var producer = new ProducerBuilder<string, int>(config).Build())
        {
            Console.WriteLine($"Kafka Producer started. Sending messages to topic: {topic}");
            Console.WriteLine("Press 'v' to send a valid range message, any other key for a random mock message. Press 'q' to quit.");

            while (true)
            {
                var key = Console.ReadKey(intercept: true).Key;

                if (key == ConsoleKey.Q)
                {
                    Console.WriteLine("Exiting application...");
                    break;
                }

                string messageJson;

                if (key == ConsoleKey.V)
                {
                    // Generate a valid range message
                    var validMessage = GenerateValidRangeMessage();
                    messageJson = JsonSerializer.Serialize(validMessage);
                    Console.WriteLine($"Sending valid range message: {messageJson}");
                }
                else
                {
                    // Generate a random mock message
                    var mockMessage = GenerateMockIotData();
                    messageJson = JsonSerializer.Serialize(mockMessage);
                    Console.WriteLine($"Sending mock message: {messageJson}");
                }

                try
                {
                    // Send message to Kafka
                    var deliveryResult = await producer.ProduceAsync(topic, new Message<string, int>
                    {
                        Value = 1
                    });

                    Console.WriteLine($"Message sent | Partition: {deliveryResult.Partition}, Offset: {deliveryResult.Offset}");
                }
                catch (ProduceException<string, int> e)
                {
                    Console.WriteLine($"Delivery failed: {e.Error.Reason}");
                }
            }
        }
    }

    static IotData GenerateMockIotData()
    {
        var random = new Random();

        return new IotData
        {
            Lat = Math.Round(10 + random.NextDouble() * 50, 2), // Random latitude (not necessarily valid)
            Long = Math.Round(10 + random.NextDouble() * 70, 2), // Random longitude (not necessarily valid)
            Speed = Math.Round(random.NextDouble() * 200, 2), // Random speed between 0 and 200
            DeviceId = Guid.NewGuid().ToString() // Random unique identifier
        };
    }

    static IotData GenerateValidRangeMessage()
    {
        var random = new Random();

        return new IotData
        {
            Lat = Math.Round(25 + random.NextDouble() * (39.8 - 25), 2), // Latitude within Iran's bounds
            Long = Math.Round(44 + random.NextDouble() * (63 - 44), 2),  // Longitude within Iran's bounds
            Speed = Math.Round(random.NextDouble() * 200, 2),           // Random speed between 0 and 200
            DeviceId = Guid.NewGuid().ToString()                        // Random unique identifier
        };
    }
}