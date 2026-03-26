using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Runtime;
using Escendit.Orleans.Streaming.RabbitMQ;
using RabbitMQ.Stream.Client;

namespace Grains;

public class HelloGrain : Grain, IHello, IAsyncObserver<string>
{
    private readonly ILogger _logger;
    private StreamSubscriptionHandle<string>? _streamSubscriptionHandle;
    private IAsyncStream<string> _stream;

    public HelloGrain(ILogger<HelloGrain> logger) => _logger = logger;

    ValueTask<string> IHello.SayHello(string greeting)
    {
        _logger.LogInformation(
            "SayHello message received: greeting = '{Greeting}'", greeting);

        _logger.LogInformation("This call happened on {Date}", DateTime.Now.ToString("yyyyMMdd"));

        return ValueTask.FromResult(
            $"""
            Client said: '{greeting}', so HelloGrain says: Hello!
            """);
    }

    //==========================================================================
   


  
    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        var streamProvider = this.GetStreamProvider("Hank");
        var stream = streamProvider.GetStream<string>("PipelineID", "bob");

        if (_streamSubscriptionHandle is null)
        {
            _streamSubscriptionHandle = await stream.SubscribeAsync(OnNextAsync);
        }
        else
        {
            await _streamSubscriptionHandle.ResumeAsync(OnNextAsync);
        }

        await base.OnActivateAsync(cancellationToken);
    }

    private Task OnNextAsync(IList<SequentialItem<string>> arg)
    {
        foreach (var item in arg)
        {
            _logger.LogInformation($"Received message from queue: {item.Item}");
        }
        SendMessage("Thanks Bob");
        return Task.CompletedTask;

    }

    public async Task SendMessage(string message)
    {
        var streamProvider = this.GetStreamProvider("Hank");
        var stream = streamProvider.GetStream<string>("PipelineID1", "bob");
        await stream.OnNextAsync(message);
    }

    public Task ReceiveMessage(string message)
    {
        _logger.LogInformation($"Received message: {message}");
        return Task.CompletedTask;
        //await _stream.OnNextAsync(42);
    }

    //public Task OnNextAsync(int item, StreamSequenceToken? token = null)
    //{
    //    Console.WriteLine(item);
    //    return Task.CompletedTask;
    //}

    public Task OnCompletedAsync() => throw new NotImplementedException();
    public Task OnErrorAsync(Exception ex) => throw new NotImplementedException();

    Task IAsyncObserver<string>.OnNextAsync(string item, StreamSequenceToken? token)
    {
        Console.WriteLine(item);
        return Task.CompletedTask;
    }
}