using Microsoft.AspNetCore.Mvc;
using Transaction.Api.DTOs;
using Transaction.Api.Services;

namespace Transaction.Api.Controllers;

[ApiController]
[Route("api/transactions")]
public class TransactionController : ControllerBase
{
    private readonly MongoDbService _mongoDbService;
    private readonly KafkaService _kafkaService;

    public TransactionController(MongoDbService mongoDbService, KafkaService kafkaService)
    {
        _mongoDbService = mongoDbService;
        _kafkaService = kafkaService;
    }

    [HttpPost]
    public async Task<ActionResult> CreateTransaction(CreateTransactionDto dto)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(dto.CustomerName))
                return BadRequest(new { message = "Customer name must be required" });

            if (string.IsNullOrWhiteSpace(dto.ProductName))
                return BadRequest(new { message = "Customer name must be required" });

            if (dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be required" });

            var transactionDto = new TransactionDto
            {
                CustomerName = dto.CustomerName,
                ProductName = dto.ProductName,
                Amount = dto.Amount,
                Status = TransactionStatus.Pending
            };

            await _mongoDbService.CreateTransactionAsync(transactionDto);

            await _kafkaService.PublishAsync(transactionDto);

            return Created();
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "Internal server erro create transaction" });
            throw;
        }
    }

    [HttpGet]
    public async Task<ActionResult<List<TransactionDto>>> GetAll()
    {
        try
        {
            return Ok(await _mongoDbService.GetAllAsync());
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            throw;
        }
    }
}
