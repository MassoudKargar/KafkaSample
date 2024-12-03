// See https://aka.ms/new-console-template for more information
using System;
using Confluent.Kafka;

class SpeedAboveMaxConsumer
{
    static void Main(string[] args)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = "localhost:9092", // Replace with your Kafka broker address
            GroupId = "speed-above-consumer-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        string topic = "SPEEDABOVEMAX";

        using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
        {
            consumer.Subscribe(topic);

            Console.WriteLine($"Subscribed to topic: {topic}");
            Console.WriteLine("Press 'q' to quit the application.");

            try
            {
                while (true)
                {
                    var result = consumer.Consume();

                    Console.WriteLine($"Message received: {result.Message.Value}");

                    // Exit on 'q' key press
                    if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Q)
                    {
                        Console.WriteLine("Exiting application...");
                        break;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Handle graceful shutdown
            }
            finally
            {
                consumer.Close();
            }
        }
    }
}
