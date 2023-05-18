using GrainInterfaces;
using Microsoft.Extensions.Logging;
using Orleans.Streams;

namespace Grains
{
    public class ConfigureOnDemand : Grain, IConfigureOnDemand, IAsyncObserver<string>
    {

        private readonly ILogger _logger;
        private StreamSubscriptionHandle<string>? _streamSubscriptionHandle;
        private IAsyncStream<string> _stream;

        public ConfigureOnDemand(ILogger<ConfigureOnDemand> logger) => _logger = logger;

        public async Task Initialize(string QueueID)
        {
            var streamProvider = this.GetStreamProvider("Hank");
            _stream = streamProvider.GetStream<string>("PipelineID", QueueID);

            if (_streamSubscriptionHandle is null)
            {
                _streamSubscriptionHandle = await _stream.SubscribeAsync(OnNextAsync);
            }
            else
            {
                await _streamSubscriptionHandle.ResumeAsync(OnNextAsync);
            }

            return;
        }


        public Task OnNextAsync(string item, StreamSequenceToken? token = null)
        {
            //_logger.LogInformation($"ConfigureOnDemand Received message from queue: {item} **************************");
            Console.WriteLine(item);
            return Task.CompletedTask;
        }

       

        public Task OnCompletedAsync()
        {
            throw new NotImplementedException();
        }

        public Task OnErrorAsync(Exception ex)
        {
            throw new NotImplementedException();
        }
    }
}
