namespace Transaction.Api.DTOs;

public class CreateTransactionDto
{
    public string CustomerName { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
