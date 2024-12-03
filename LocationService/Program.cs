// See https://aka.ms/new-console-template for more information
using Common;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;

class Program
{
    static async Task Main(string[] args)
    {
        // Kafka configuration
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092", // Replace with your Kafka broker address
            GroupId = "state-detector-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092" ,// Replace with your Kafka broker address
        };

        string inputTopic = "IOTREPORTDATAVALID";
        string outputTopic = "Location";

        // Create Kafka consumer
        using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
        using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
        {
            consumer.Subscribe(inputTopic);

            Console.WriteLine($"Subscribed to topic: {inputTopic}");
            Console.WriteLine($"Publishing results to topic: {outputTopic}");
            Console.WriteLine("Press 'q' to quit the application.");

            try
            {
                while (true)
                {
                    // Poll for messages
                    var result = consumer.Consume();

                    // Deserialize the message to IotData
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
                        var iotData = JsonConvert.DeserializeObject<IotData>(result.Message.Value, settings);

                        // Detect the state based on latitude and longitude
                        var state = DetectState(iotData.Lat, iotData.Long);

                        // Create the output message
                        var locationMessage = new LocationData(iotData.DeviceId, state);

                        var locationJson = JsonConvert.SerializeObject(locationMessage);

                        // Publish the result to the Location topic
                        await producer.ProduceAsync(outputTopic, new Message<Null, string>
                        {
                            Value = locationJson
                        });

                        Console.WriteLine($"Processed: {result.Message.Value} => {locationJson}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to process message: {result.Message.Value}. Error: {ex.Message}");
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

    static string DetectState(double latitude, double longitude)
    {
        // Dummy implementation of state detection
        if (latitude >= 35.5 && latitude <= 36.5 && longitude >= 51.0 && longitude <= 52.0)
            return "Tehran";
        if (latitude >= 29.5 && latitude <= 30.5 && longitude >= 49.0 && longitude <= 50.0)
            return "Shiraz";
        if (latitude >= 31.0 && latitude <= 32.0 && longitude >= 47.0 && longitude <= 48.0)
            return "Ahvaz";

        return "Unknown";
    }
}
