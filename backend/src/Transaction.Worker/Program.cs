using Transaction.Worker;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<TransactionWorker>();

var host = builder.Build();
host.Run();
