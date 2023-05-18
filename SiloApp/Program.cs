using Escendit.Orleans.Streaming.RabbitMQ.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Orleans.Hosting;

try
{
    using IHost host = await StartSiloAsync();
    Console.WriteLine("\n\n Press Enter to terminate...\n\n");
    Console.ReadLine();

    await host.StopAsync();

    return 0;
}
catch (Exception ex)
{
    Console.WriteLine(ex);
    return 1;
}

static async Task<IHost> StartSiloAsync()
{   
    var builder = new HostBuilder()
        
        .UseOrleans(silo =>
        {
            silo.UseLocalhostClustering()
                .ConfigureLogging(logging => logging.AddConsole())
                .AddMemoryGrainStorageAsDefault()
                .AddMemoryGrainStorage("PubSubStore")
                .AddStreaming()
                .AddRabbitMqStreaming("Hank")
                .WithQueue(options =>
                {
                    options.Endpoints.Add(new RabbitEndpoint { HostName = "localhost", Port = 5672 });
                    options.UserName = "guest";
                    options.Password = "guest";
                    options.VirtualHost = "/";
                    options.ClientProvidedName = "Silo-Queue";
                });
        });

    var host = builder.Build();
    await host.StartAsync();

    return host;
}