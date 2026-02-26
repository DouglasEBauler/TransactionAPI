using MongoDB.Driver;
using System.Reflection;
using Transaction.Api.DTOs;

namespace Transaction.Api.Services;

public class MongoDbService
{
    private readonly IMongoCollection<TransactionDto> _transactions;

    public MongoDbService(IConfiguration config)
    {
        var mongoClient = new MongoClient(config["MongoDB:ConnectionString"]);
        var database = mongoClient.GetDatabase(config["MongoDB:DatabaseName"]);
        _transactions = database.GetCollection<TransactionDto>("transactions");

        var indexModel = new CreateIndexModel<TransactionDto>(
            Builders <TransactionDto>.IndexKeys.Ascending(t => t.TransactionId),
            new CreateIndexOptions { Unique = true }
        );

        _transactions.Indexes.CreateOne(indexModel);
    }
    
    public async Task CreateTransactionAsync(TransactionDto transactionDto)
    {
        await _transactions.InsertOneAsync(transactionDto);
    }

    public async Task<List<TransactionDto>> GetAllAsync()
    {
        return await _transactions
            .Find(Builders<TransactionDto>.Filter.Empty)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    }
}
