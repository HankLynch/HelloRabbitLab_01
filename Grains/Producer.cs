using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Streams;
using Orleans.Runtime;
using Escendit.Orleans.Streaming.RabbitMQ;
using RabbitMQ.Stream.Client;

namespace Grains;

public class ProducerGrain : Grain, IProducerGrain, IAsyncObserver<string>
{
    private readonly ILogger _logger;
    private StreamSubscriptionHandle<string>? _streamSubscriptionHandle;
    private IAsyncStream<string> _stream;
    private IStreamProvider _streamProvider;

    public ProducerGrain(ILogger<HelloGrain> logger) => _logger = logger;



    //==========================================================================




    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        _streamProvider = this.GetStreamProvider("Hank");
        _stream = _streamProvider.GetStream<string>("PipelineID", "bob1");           //     , Guid.Empty);

        if (_streamSubscriptionHandle is null)
        {
            _streamSubscriptionHandle = await _stream.SubscribeAsync(OnNextAsync);
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
            _logger.LogInformation($"Received message but shouldn't: {item.Item}");
        }
        return Task.CompletedTask;

    }

    public async Task SendMessage(string message)
    {
        var streamProvider = this.GetStreamProvider("Hank");
        var stream = streamProvider.GetStream<string>("PipelineID", "bob");  //this targets the next grain in the pipeline
        await stream.OnNextAsync(message);
    }

    public Task ReceiveMessage(string message)
    {
        _logger.LogInformation($"I shouldn't get this: {message}");
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