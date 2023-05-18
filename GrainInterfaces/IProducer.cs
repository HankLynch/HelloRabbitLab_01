namespace GrainInterfaces;

public interface IProducerGrain : IGrainWithIntegerKey
{
      Task SendMessage(string message);
}