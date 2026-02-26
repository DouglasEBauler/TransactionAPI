using Confluent.Kafka;
using System.Text.Json;
using Transaction.Api.DTOs;

namespace Transaction.Api.Services;

public class KafkaService
{
    private readonly IProducer<string, string> _producer;

    public KafkaService(IConfiguration config)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = config["Kafka:BootstrapServers"],
            ClientId = "transaction-api",
            Acks = Acks.All,
            MessageTimeoutMs = 10000
        };

        _producer = new ProducerBuilder<string, string>(producerConfig).Build();
    }

    public async Task PublishAsync(TransactionDto dto)
    {
        try
        {
            var json = JsonSerializer.Serialize(dto);

            var message = new Message<string, string>
            {
                Key = Guid.NewGuid().ToString(),
                Value = json,
                Timestamp = Timestamp.Default
            };

            var result = await _producer.ProduceAsync("transactions_topic", message);
        }
        catch (ProduceException<string, string> ex)
        {
            Console.WriteLine($"Kafka Producer Error: {ex.Error.Reason}");
            throw;
        }
    }
}
