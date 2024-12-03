using Common;
using Confluent.Kafka;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text.Json;
public class Program
{
    static async Task Main(string[] args)
    {
        // Kafka configuration for consumer
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092", // Replace with your Kafka broker address
            GroupId = "max-speed-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        // Kafka configuration for producer
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = "localhost:9092" // Replace with your Kafka broker address
        };

        string inputTopic = "LocationTopic";
        string outputTopic = "Speed";

        // Dictionary to track max speed per state
        var maxSpeedByState = new Dictionary<string, double> {

        {"Tehran",90 },
        {"Shiraz",80 },
        {"Ahvaz",50 },
        {"Unknown",100},
    };

        // Create Kafka consumer and producer
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
                        // Deserialize the message from JSON
                        var locationMessage = JsonConvert.DeserializeObject<LocationData>(result.Message.Value);

                        if (locationMessage != null)
                        {
                            var state = locationMessage.State;
                            var deviceId = locationMessage.DeviceId;
                            double maxSpeed;
                            // Update max speed for the state
                            if (!maxSpeedByState.ContainsKey(state))
                            {
                                maxSpeed = 100;
                            }
                            else
                            {
                                maxSpeed = maxSpeedByState[state];
                            }
                            var maxSpeedMessage = new SpeedClass(
    maxSpeed,
    deviceId
                            );

                            // Serialize and produce the result
                            var maxSpeedJson = JsonConvert.SerializeObject(maxSpeedMessage);

                            await producer.ProduceAsync(outputTopic, new Message<Null, string>
                            {
                                Value = maxSpeedJson
                            });

                            Console.WriteLine($"Updated Max Speed: {maxSpeedJson}");
                        }
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

}
