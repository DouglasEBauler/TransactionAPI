using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Transaction.Api.DTOs;

public class TransactionDto
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string TransactionId { get; set; } = Guid.NewGuid().ToString();
    public string ProductName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public TransactionStatus Status { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? ErrorMessage { get; set; }
}

public enum TransactionStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    Failed = 3
}
