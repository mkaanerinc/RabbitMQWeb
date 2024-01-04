using FileCreateWorkerService;
using FileCreateWorkerService.Services;
using RabbitMQ.Client;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<RabbitMQClientService>();
builder.Services.AddSingleton(sp => new ConnectionFactory()
{
    Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")),
    DispatchConsumersAsync = true,
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
