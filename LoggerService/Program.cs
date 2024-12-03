using Common;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;

class Program
{
    static void Main(string[] args)
    {
        // Kafka configuration
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092", // Replace with your Kafka broker address
            GroupId = "iot-report-consumer-group", // Consumer group ID
            AutoOffsetReset = AutoOffsetReset.Earliest, // Start from the earliest message
            EnableAutoCommit = true // Automatically commit offsets
        };

        string topic = "IOTREPORTDATAVALID";

        // Create Kafka consumer
        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(topic);

            Console.WriteLine($"Subscribed to topic: {topic}");
            Console.WriteLine("Press 'q' to quit the application.");

            try
            {
                while (true)
                {
                    // Poll for messages
                    var result = consumer.Consume(CancellationToken.None);

                    // Log the message to the console
                    Console.WriteLine($"Received message: {result.Message.Value}");

                    // Optional: Deserialize the message to an object
                    try
                    {
                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = new DefaultContractResolver
                            {
                                NamingStrategy = new CamelCaseNamingStrategy
                                {
                                    ProcessDictionaryKeys = true,
                                    OverrideSpecifiedNames = true
                                }
                            }
                        };
                        var iotData = JsonConvert.DeserializeObject<IotData>(result.Message.Value,settings);
                        Console.WriteLine($"Deserialized Data - Lat: {iotData.Lat}, Long: {iotData.Long}, Speed: {iotData.Speed}, DeviceId: {iotData.DeviceId}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to deserialize message: {ex.Message}");
                    }

                    // Check if 'q' is pressed to quit
                    if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("Exiting application...");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
