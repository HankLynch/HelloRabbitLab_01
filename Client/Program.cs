using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using GrainInterfaces;
using Orleans.Configuration;
using Escendit.Orleans.Streaming.RabbitMQ.Options;

try
{
    Console.WriteLine("press any key after silo launches");
    Console.ReadKey();
    using IHost host = await StartClientAsync();
    var client = host.Services.GetRequiredService<IClusterClient>();

    await DoClientWorkAsync(client);
    Console.ReadKey();

    await host.StopAsync();

    return 0;
}
catch (Exception e)
{
    Console.WriteLine($$"""
        Exception while trying to run client: {{e.Message}}
        Make sure the silo the client is trying to connect to is running.
        Press any key to exit.
        """);

    Console.ReadKey();
    return 1;
}

static async Task<IHost> StartClientAsync()
{
    var builder = new HostBuilder()
        .UseOrleansClient(client =>
        {
            client.UseLocalhostClustering()
             .Configure<ClusterOptions>(options =>
             {
                 options.ClusterId = "default";
                 options.ServiceId = "service-id";
             })
            .AddStreaming()
            .AddRabbitMqStreaming("ProviderName")
            .WithQueue(options =>
            {
                options.Endpoints.Add(new RabbitEndpoint { HostName = "localhost", Port = 5672 });
                options.UserName = "guest";
                options.Password = "guest";
                options.VirtualHost = "/";
            })
            ;
        })
        .ConfigureLogging(logging => logging.AddConsole());

    var host = builder.Build();
    await host.StartAsync();

    Console.WriteLine("Client successfully connected to silo host \n");

    return host;
}

static async Task DoClientWorkAsync(IClusterClient client)
{
    var configurable = client.GetGrain<IConfigureOnDemand>("NamedGrain");
    await configurable.Initialize("bobo");

    var friend = client.GetGrain<IHello>(0);
    var response = await friend.SayHello("Good morning, HelloGrain!");

    Console.WriteLine($"\n\n{response}\n\n");
    var sender = client.GetGrain<IProducerGrain>(0);
    await sender.SendMessage(DateTime.Now.Second.ToString() +":"+ DateTime.Now.Millisecond.ToString());
    for (int i = 0; i < 100; i++)
    {
        await sender.SendMessage($"Message {i}");
    }
    await sender.SendMessage("Sender says hells yeah!");
    await sender.SendMessage(DateTime.Now.Second.ToString() + ":" + DateTime.Now.Millisecond.ToString());

    Console.WriteLine("Message Sent");

}