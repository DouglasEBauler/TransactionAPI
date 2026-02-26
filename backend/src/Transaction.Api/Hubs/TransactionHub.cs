using Microsoft.AspNetCore.SignalR;

namespace Transaction.Api.Hubs;

public class TransactionHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendTransactionUpdate(object transaction)
    {
        await Clients.All.SendAsync("ReceiveTransactionUpdate", transaction);
    }
}
