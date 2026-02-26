using Confluent.Kafka;
using Microsoft.AspNetCore.SignalR.Client;
using MongoDB.Driver;
using System.Reflection;
using System.Text.Json;
using Transaction.Worker.DTOs;

namespace Transaction.Worker;

public class TransactionWorker : BackgroundService
{
    private readonly ILogger<TransactionWorker> _logger;
    private readonly IConfiguration _config;
    private IConsumer<string, string> _consumer;
    private IMongoCollection<TransactionDto> _transactions;
    private HubConnection _connection;

    public TransactionWorker(ILogger<TransactionWorker> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _config["Kafka:BootstrapServers"],
            GroupId = "transaction-process-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            SessionTimeoutMs = 10000
        };

        _consumer = new ConsumerBuilder<string, string>(consumerConfig).Build();
        _consumer.Subscribe("transactions_topic");

        var mongoClient = new MongoClient(_config["MongoDB:ConnectionString"]);
        var database = mongoClient.GetDatabase(_config["MongoDB:DatabaseName"]);
        _transactions = database.GetCollection<TransactionDto>("transactions");

        _connection = new HubConnectionBuilder()
            .WithUrl(_config["SignalR:HubUrl"]!)
            .WithAutomaticReconnect()
            .Build();

        _connection.StartAsync().Wait();
        _logger.LogInformation("SignalR connected");
  
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            try
            {
                await ProcessMessage();
            }
            catch (ConsumeException ex)
            {
               throw;
            }
            catch (Exception ex)
            {
                throw;
            }

            await Task.Delay(1000, stoppingToken);
        }
    }

    private async Task ProcessMessage()
    {
        var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

        if (consumeResult != null)
        {
            var message = consumeResult.Message.Value;

            var transactionDto = JsonSerializer.Deserialize<TransactionDto>(message);

            if (transactionDto != null)
            {
                transactionDto.Status = TransactionStatus.Processing;

                await UpdateTransaction(transactionDto);
                await NotifyHub(transactionDto);

                await Task.Delay(Random.Shared.Next(2000, 5000));

                try
                {
                    transactionDto.Status = TransactionStatus.Completed;
                    transactionDto.ProcessedAt = DateTime.UtcNow;
                    await UpdateTransaction(transactionDto);
                    await NotifyHub(transactionDto);
                }
                catch (Exception)
                {
                    transactionDto.Status = TransactionStatus.Failed;
                    transactionDto.ErrorMessage = "Failed Process transaction";
                    await UpdateTransaction(transactionDto);
                    await NotifyHub(transactionDto);

                    throw;
                }

                _consumer.Commit(consumeResult);
            }
        }
    }

    private async Task UpdateTransaction(TransactionDto dto)
    {
        var filter = Builders<TransactionDto>.Filter.Eq(t => t.TransactionId, dto.TransactionId);
        var update = Builders<TransactionDto>.Update
            .Set(t => t.CustomerName, dto.CustomerName)
            .Set(t => t.ProductName, dto.ProductName)
            .Set(t => t.Amount, dto.Amount)
            .Set(t => t.Status, dto.Status);

        await _transactions.UpdateOneAsync(filter, update);
    }

    private async Task NotifyHub(TransactionDto dto)
    {
        await _connection.InvokeAsync("SendTransactionUpdate", dto);
    }
}
